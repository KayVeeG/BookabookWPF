using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BookabookWPF.Attributes;
using BookabookWPF.Services;

namespace BookabookWPF.Pages
{
    public class EditPage : Page
    {
        public static readonly DependencyProperty ModelInstancesProperty =
            DependencyProperty.Register(nameof(ModelInstances), typeof(object), typeof(EditPage),
                new PropertyMetadata(null, OnModelInstancesChanged));

        public IList<object> ModelInstances
        {
            get => (IList<object>)GetValue(ModelInstancesProperty);
            set
            {
                SetValue(ModelInstancesProperty, value);
                ModelInstancesBackup = ModelInstances?.ToList();
                if (ModelInstancesBackup is not null)
                {
                    for (int i = 0; i < ModelInstancesBackup.Count; i++)
                    {
                        if (ModelInstancesBackup[i] is ICloneable cloneable)
                        {
                            ModelInstancesBackup[i] = cloneable.Clone();
                        }
                    }
                }
            }
        }

        protected IList<object>? ModelInstancesBackup { get; set; }

        public EditPage()
        {
            Content = new StackPanel
            {
                Margin = new Thickness(10)
            };
        }

        private static void OnModelInstancesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditPage editPage && e.NewValue != e.OldValue && e.NewValue is not null)
            {
                editPage.InitializeUI();
            }
        }

        protected void InitializeUI()
        {
            // Get the stack panel
            var stackPanel = (StackPanel)Content;
            stackPanel.Children.Clear();

            // Check if data is available
            if (ModelInstances is null || ModelInstances.Count == 0) return;

            // Get each data property of model type
            var properties = ((ModelBase)ModelInstances[0]).GetDataProperties();

            // Create controls for each property
            foreach (var property in properties)
            {

                // Create a container for each property
                var propertyContainer = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                // Check if the property should be auto-enabled
                bool autoEnable = ShouldAutoEnableEdit(property);

                // Create a label for the property
                var label = new Label
                {
                    Content = property.Name + ":",
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Create the appropriate control based on property type and attributes
                FrameworkElement control = CreateControlForProperty(property);
                control.Width = 200;
                control.VerticalAlignment = VerticalAlignment.Center;
                control.Tag = property;

                // Create checkbox for multiple instances
                CheckBox? checkBox = null;
                if (ModelInstances.Count > 1)
                {
                    checkBox = new()
                    {
                        IsChecked = autoEnable,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = control
                    };
                    checkBox.Checked += OnCheckBoxCheckedChanged;
                    checkBox.Unchecked += OnCheckBoxCheckedChanged;
                }

                // Create enable/disable binding
                Binding activationBinding = new()
                {
                    Source = checkBox,
                    Path = new PropertyPath(nameof(CheckBox.IsChecked)),
                };

                // Add bindings
                label.SetBinding(IsEnabledProperty, activationBinding);
                control.SetBinding(IsEnabledProperty, activationBinding);

                // Add controls
                if (checkBox is not null)
                    propertyContainer.Children.Add(checkBox);
                propertyContainer.Children.Add(label);
                propertyContainer.Children.Add(control);

                stackPanel.Children.Add(propertyContainer);
            }

            // Make undo button and add it to the container
            var undoButton = new Button
            {
                Content = "Undo all edits",
                Margin = new Thickness(0, 10, 0, 0)
            };
            undoButton.Click += UndoButton_Click;
            stackPanel.Children.Add(undoButton);
        }

        private FrameworkElement CreateControlForProperty(PropertyInfo property)
        {
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var value = property.GetValue(ModelInstances[0])?.ToString() ?? string.Empty;

            // Check for MultipleInDatabase attribute (ComboBox)
            if (property.GetCustomAttribute<MultipleInDatabaseAttribute>() != null)
            {
                var comboBox = new ComboBox()
                {
                    // Set the value
                    Text = value,
                    // Make it user editable
                    IsEditable = true,
                    IsTextSearchEnabled = true,
                    // Use distinct values from database
                    ItemsSource = Globals.Database!.GetDistinctValues(property.DeclaringType!.Name, property.Name)
                };

                // Subscribe to text changed event
                comboBox.Loaded += (s, e) =>
                {
                    if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                    {
                        textBox.TextChanged += (sender, args) =>
                        {
                            if (comboBox.Tag is PropertyInfo prop)
                            {
                                UpdatePropertyValue(prop, textBox.Text);
                            }
                        };
                    }
                };

                return comboBox;
            }

            // Handle different property types
            switch (Type.GetTypeCode(propertyType))
            {
                case TypeCode.DateTime:
                    var datePicker = new DatePicker();
                    if (DateTime.TryParse(value, out DateTime dateValue))
                    {
                        datePicker.SelectedDate = dateValue;
                    }
                    datePicker.SelectedDateChanged += OnDatePickerChanged;
                    return datePicker;

                case TypeCode.Decimal:
                    var numericUpDown = new Xceed.Wpf.Toolkit.DoubleUpDown
                    {
                        Value = decimal.TryParse(value, out decimal decimalValue) ? (double)decimalValue : 0.0,
                        FormatString = "F2",  // Display 2 decimal places
                        Minimum = 0,
                        Increment = 0.01
                    };
                    numericUpDown.ValueChanged += (s, e) =>
                    {
                        if (s is Xceed.Wpf.Toolkit.DoubleUpDown nud && nud.Tag is PropertyInfo prop)
                        {
                            UpdatePropertyValue(prop, nud.Value.ToString()!);
                        }
                    };
                    return numericUpDown;
                case TypeCode.Int32:
                    var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
                    if (rangeAttr != null)
                    {

                        // Make container grid
                        Grid grid = new()
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Auto)},
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)}
                            },
                        };

                        // Make slider
                        Slider slider = new()
                        {
                            Minimum = Convert.ToDouble(rangeAttr.Minimum),
                            Maximum = Convert.ToDouble(rangeAttr.Maximum),
                            Value = int.TryParse(value, out int intValue) ? intValue : Convert.ToDouble(rangeAttr.Minimum),
                            IsSnapToTickEnabled = true,
                            TickFrequency = 1,
                            Tag = property
                        };
                        // Subscribe to value changed event
                        slider.ValueChanged += OnSliderValueChanged;

                        // Make value displaying label
                        TextBlock label = new();
                        label.SetBinding(TextBlock.TextProperty, new Binding()
                        {
                            Source = slider,
                            Path = new PropertyPath(nameof(Slider.Value)),
                            StringFormat = "{0:F0}",
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = BindingMode.OneWay,
                            // Add binding error handler
                            NotifyOnSourceUpdated = true,
                            NotifyOnTargetUpdated = true
                        });

                        // Add controls to grid
                        label.SetValue(Grid.ColumnProperty, 0);
                        slider.SetValue(Grid.ColumnProperty, 1);
                        grid.Children.Add(label);
                        grid.Children.Add(slider);
                        return grid;
                    }
                    goto default;

                default:
                    var textBox = new TextBox { Text = value };
                    textBox.TextChanged += OnTextBoxChanged;
                    return textBox;
            }
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Tag is PropertyInfo property)
            {
                UpdatePropertyValue(property, comboBox.SelectedItem?.ToString() ?? string.Empty);
            }
        }

        private void OnDatePickerChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.Tag is PropertyInfo property)
            {
                UpdatePropertyValue(property, datePicker.SelectedDate?.ToString() ?? string.Empty);
            }
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && slider.Tag is PropertyInfo property)
            {
                UpdatePropertyValue(property, ((int)slider.Value).ToString());
            }
        }

        // Existing methods remain the same
        protected bool ShouldAutoEnableEdit(PropertyInfo property)
        {
            // Check if data is available
            if (ModelInstances is null || ModelInstances.Count == 0 || property is null) return false;

            // Use the first instance as comparable reference
            object? value = property.GetValue(ModelInstances[0]);

            // Compare all with the first
            foreach (var instance in ModelInstances)
            {
                // Get the other value to compare
                object? otherValue = property.GetValue(instance);
                // Return when slightest change is detected
                if (value is null && otherValue is not null)
                    return false;
                else if (value is not null && otherValue is null)
                    return false;
                else if (value is not null && !value.Equals(otherValue))
                    return false;
            }

            // Return true when all values are equal
            return true;

        }

        private void OnTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is PropertyInfo property)
            {
                UpdatePropertyValue(property, textBox.Text);
            }
        }

        private void OnCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is FrameworkElement control &&
                control.Tag is PropertyInfo property)
            {
                if (checkBox.IsChecked == true)
                {
                    string value = GetControlValue(control)!;
                    UpdatePropertyValue(property, value);
                }
                else
                {
                    string backupValue = property.GetValue(ModelInstancesBackup![0])?.ToString() ?? string.Empty;
                    SetControlValue(control, backupValue);
                }
            }
        }

        private string? GetControlValue(FrameworkElement control)
        {
            return control switch
            {
                TextBox textBox => textBox.Text,
                ComboBox comboBox => comboBox.Text ?? string.Empty,
                DatePicker datePicker => datePicker.SelectedDate?.ToString() ?? string.Empty,
                Slider slider => ((int)slider.Value).ToString(),
                Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown => numericUpDown.Value.ToString(),
                _ => string.Empty
            };
        }

        private void SetControlValue(FrameworkElement control, string value)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.Text = value;
                    break;
                case ComboBox comboBox:
                    comboBox.Text = value;
                    break;
                case DatePicker datePicker:
                    if (DateTime.TryParse(value, out DateTime dateValue))
                        datePicker.SelectedDate = dateValue;
                    break;
                case Slider slider:
                    if (int.TryParse(value, out int intValue))
                        slider.Value = intValue;
                    break;
                case Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown:
                    if (double.TryParse(value, out double doubleValue))
                        numericUpDown.Value = doubleValue;
                    break;
            }
        }

        protected void UpdatePropertyValue(PropertyInfo property, string value)
        {
            foreach (var instance in ModelInstances)
            {
                var setter = property.GetSetMethod();
                if (setter != null)
                {
                    object? convertedValue = ConvertValue(value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        setter.Invoke(instance, new[] { convertedValue });
                    }
                }
            }
        }

        private object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return underlyingType.Name switch
            {
                nameof(String) => value,
                nameof(DateTime) => DateTime.TryParse(value, out DateTime dateValue) ? dateValue : null,
                nameof(Decimal) => decimal.TryParse(value, out decimal decimalValue) ?
                    Math.Round(decimalValue, 2) : null,
                nameof(Int32) => int.TryParse(value, out int intValue) ? intValue : null,
                _ => null
            };
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModelInstancesBackup is null || ModelInstances is null) return;

            var stackPanel = (StackPanel)Content;
            foreach (StackPanel propertyContainer in stackPanel.Children.OfType<StackPanel>())
            {
                var control = propertyContainer.Children.OfType<FrameworkElement>()
                    .FirstOrDefault(c => c.Tag is PropertyInfo);

                if (control?.Tag is PropertyInfo property)
                {
                    var backupValue = property.GetValue(ModelInstancesBackup[0])?.ToString() ?? string.Empty;
                    SetControlValue(control, backupValue);
                }
            }
        }
    }
}