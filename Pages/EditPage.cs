using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                // Backup the model instances
                ModelInstancesBackup = ModelInstances?.ToList();
                if (ModelInstancesBackup is not null)
                {
                    for (int i = 0; i < ModelInstancesBackup.Count; i++)
                    {
                        if (ModelInstancesBackup[i] is ICloneable cloneable)
                        {
                            // Clone the model instances
                            ModelInstancesBackup[i] = cloneable.Clone();
                        }
                        else
                        {
                            throw new InvalidOperationException("Model must implement ICloneable");
                        }
                    }
                }
            }
        }

        // List that stores model instances backup for undo
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
                StackPanel propertyContainer = new()
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
                    // Subscribe to checked changed event
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

                // Add controls to property container
                if (checkBox is not null)
                    propertyContainer.Children.Add(checkBox);
                propertyContainer.Children.Add(label);
                propertyContainer.Children.Add(control);

                // Add property container to stack panel
                stackPanel.Children.Add(propertyContainer);
            }

            // Make undo button
            var undoButton = new Button
            {
                Content = "Undo all edits",
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Subscribe to click event
            undoButton.Click += UndoButton_Click;

            // Add undo button to stack panel
            stackPanel.Children.Add(undoButton);
        }

        private FrameworkElement CreateControlForProperty(PropertyInfo property)
        {
            #region VariablePreperation
            // Abstract the property type
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // Extract the property value from first element
            object? value = property.GetValue(ModelInstances[0]);
            #endregion VariablePreperation

            #region TypeSwitching
            // Handle different property types
            switch (Type.GetTypeCode(propertyType))
            {
                #region DateTime
                // Case of DateTime
                case TypeCode.DateTime:
                    #region DateTime control generation
                    // Make date picker
                    var datePicker = new DatePicker()
                    {
                        SelectedDate = (DateTime?)value, // Set the value
                    };
                    #endregion DateTime control generation

                    #region DateTime event subscription
                    // Subscribe to value changed event
                    datePicker.SelectedDateChanged += (s, e) =>
                    {
                        if (s is DatePicker datePicker && datePicker.Tag is PropertyInfo property)
                        {
                            // Update the property value
                            UpdatePropertyValues(property, datePicker.SelectedDate);
                        }
                    };
                    // Subscribe to text changed event
                    datePicker.Loaded += (s, e) => {
                        var textBox = datePicker.Template.FindName("PART_TextBox", datePicker) as DatePickerTextBox;
                        if (textBox != null)
                        {
                            textBox.TextChanged += (sender, args) => {
                                // If text input is a valid date
                                if (DateTime.TryParse(textBox.Text, out DateTime date))
                                {
                                    // Update the property value
                                    UpdatePropertyValues(property, date);
                                }
                            };
                        }
                    };
                    #endregion DateTime event subscription

                    #region DateTime return
                    // Return the date picker
                    return datePicker;
                #endregion DateTime return
                #endregion DateTime

                #region Decimal
                // Case of decimal value (such as price for example)
                case TypeCode.Decimal:
                    #region Decimal control generation
                    // Make double up down with third party library
                    var doubleUpDown = new Xceed.Wpf.Toolkit.DoubleUpDown
                    {
                        Value = Convert.ToDouble(value), // Set the value
                        FormatString = "F2",  // Display 2 decimal places
                        Minimum = 0, // Minimum value
                        Increment = 0.01 // Increment value of 1 cent
                    };
                    #endregion Decimal control generation

                    #region Decimal event subscription
                    // Subscribe to value changed event
                    doubleUpDown.ValueChanged += (s, e) =>
                    {
                        if (s is Xceed.Wpf.Toolkit.DoubleUpDown nud && nud.Tag is PropertyInfo prop)
                        {
                            // Update the property value
                            UpdatePropertyValues(prop, (decimal?)nud.Value);
                        }
                    };
                    #endregion Decimal event subscription

                    #region Decimal return
                    // Return the double up down
                    return doubleUpDown;
                #endregion Decimal return
                #endregion Decimal

                #region Int32
                // Case of integer value
                case TypeCode.Int32:
                    #region Int32 RangeAttribute check
                    // Check for Range attribute
                    var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
                    if (rangeAttr != null)
                    {
                        #region Int32 Range control generation
                        // Make container grid
                        Grid grid = new()
                        {
                            // Define layout columns
                            ColumnDefinitions =
                            {
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Auto)}, // Auto width for label
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)} // Star width for slider
                            }
                        };

                        // Make slider
                        Slider slider = new()
                        {
                            Minimum = Convert.ToDouble(rangeAttr.Minimum), // Extract minimum value from range attribute
                            Maximum = Convert.ToDouble(rangeAttr.Maximum), // Extract maximum value from range attribute
                            Value = value is null ? Convert.ToDouble(rangeAttr.Minimum) : Convert.ToDouble(value), // Set the value depending on property value availability
                            IsSnapToTickEnabled = true, // Snap to tick
                            TickFrequency = 1, // Tick frequency is 1 since it's an integer
                        };
                        // Make value displaying label
                        TextBlock label = new();
                        // Bind the label text to slider value
                        label.SetBinding(TextBlock.TextProperty, new Binding()
                        {
                            Source = slider, // Source is the slider
                            Path = new PropertyPath(nameof(Slider.Value)), // Path is the value property of the slider
                            StringFormat = "{0:F0}", // Format the value to integer
                        });
                        // Set the grid column for label and slider
                        label.SetValue(Grid.ColumnProperty, 0);
                        slider.SetValue(Grid.ColumnProperty, 1);

                        // Add controls to grid
                        grid.Children.Add(label);
                        grid.Children.Add(slider);
                        #endregion Int32 Range control generation

                        #region Int32 Range event subscription
                        // Subscribe to value changed event
                        slider.ValueChanged += (s, e) => {
                            if (s is Slider slider && ((FrameworkElement)slider.Parent).Tag is PropertyInfo property)
                            {
                                // Update the property value
                                UpdatePropertyValues(property, Convert.ToInt32(slider.Value));
                            }
                        };
                        #endregion Int32 Range event subscription

                        #region Int32 Range return
                        // Return the grid
                        return grid;
                        #endregion Int32 Range return
                    }
                    else
                    {
                        goto default;
                    }
                #endregion Int32 RangeAttribute check
                #endregion Int32

                #region Default
                // Default case is for string value
                default:
                    #region Default MultipleInDatabaseAttribute check
                    if (property.GetCustomAttribute<MultipleInDatabaseAttribute>() != null)
                    {
                        #region Default MultipleInDatabase control generation
                        // Make combo box
                        var comboBox = new ComboBox()
                        {
                            Text = (string?)value, // Set the value
                            IsEditable = true, // Make it user editable
                            IsTextSearchEnabled = true, // Enable text search
                            ItemsSource = Globals.Database!.GetDistinctValues(property.DeclaringType!.Name, property.Name) // Use distinct values from database
                        };
                        #endregion Default MultipleInDatabase control generation

                        #region Default MultipleInDatabase event subscription
                        // Subscribe to text changed event
                        comboBox.Loaded += (s, e) =>
                        {
                            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                            {
                                textBox.TextChanged += (sender, args) =>
                                {
                                    if (comboBox.Tag is PropertyInfo prop)
                                    {
                                        UpdatePropertyValues(prop, textBox.Text);
                                    }
                                };
                            }
                        };
                        #endregion Default MultipleInDatabase event subscription

                        #region Default MultipleInDatabase return
                        return comboBox;
                        #endregion Default MultipleInDatabase return
                    }
                    #endregion Default MultipleInDatabaseAttribute check
                    else
                    {
                        #region Default TextBox
                        #region Default control generation
                        // Make text box
                        var textBox = new TextBox { Text = (string?)value };
                        #endregion Default control generation

                        #region Default event subscription
                        // Subscribe to text changed event
                        textBox.TextChanged += (s, e) =>
                        {
                            if (s is TextBox textBox && textBox.Tag is PropertyInfo property)
                            {
                                // Update the property value
                                UpdatePropertyValues(property, textBox.Text);
                            }
                        };
                        #endregion Default event subscription

                        #region Default return
                        // Return the text box
                        return textBox;
                        #endregion Default return
                        #endregion Default TextBox
                    }
                    #endregion Default
            }
            #endregion TypeSwitching
        }

        private void OnCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is FrameworkElement control &&
                control.Tag is PropertyInfo property)
            {
                // If checkbox is checked
                if (checkBox.IsChecked == true)
                {
                    // Get the control value
                    object? value = GetControlValue(control);
                    // Apply the propety value on all instances
                    UpdatePropertyValues(property, value);
                }
                else
                {
                    // Get the first backup value
                    object firstBackupValue = property.GetValue(ModelInstancesBackup![0])?.ToString() ?? string.Empty;
                    // Set the control value to the first backup value
                    SetControlValue(control, firstBackupValue);
                    // Load the backup for the property values independently from the first
                    LoadBackupForPropertyValue(property);
                }
            }
        }

        private object? GetControlValue(FrameworkElement control)
        {
            return control switch
            {
                TextBox textBox => textBox.Text,
                ComboBox comboBox => comboBox.Text,
                DatePicker datePicker => datePicker.SelectedDate,
                Grid sliderGrid => Convert.ToInt32(sliderGrid.Children.OfType<Slider>().First().Value),
                Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown => (decimal?)numericUpDown.Value,
                _ => null
            };
        }

        private void SetControlValue(FrameworkElement control, object? value)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.Text = (string?)value;
                    break;
                case ComboBox comboBox:
                    comboBox.Text = (string?)value;
                    break;
                case DatePicker datePicker:
                    datePicker.SelectedDate = (DateTime?)value;
                    break;
                case Grid sliderGrid:
                    sliderGrid.Children.OfType<Slider>().First().Value = Convert.ToDouble(value);
                    break;
                case Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown:
                    numericUpDown.Value = Convert.ToDouble(value);
                    break;
            }
        }

        protected void UpdatePropertyValues(PropertyInfo property, object? value)
        {
            foreach (var instance in ModelInstances)
            {
                MethodInfo? setter = property.GetSetMethod();
                if (setter != null)
                {
                    setter.Invoke(instance, new[] { value });
                }
            }
        }

        protected void LoadBackupForPropertyValue(PropertyInfo property)
        {
            for (int i = 0; i < ModelInstances.Count; i++)
            {
                // Get the backup value
                object? backupValue = property.GetValue(ModelInstancesBackup![i] is ICloneable cloneable
                    ? cloneable.Clone() // Clone the model instance
                    : throw new InvalidOperationException("Model must implement ICloneable")); // Throw exception if model does not implement ICloneable

                // Get the setter method
                MethodInfo? setter = property.GetSetMethod();
                if (setter != null)
                {
                    // Set the property value
                    setter.Invoke(ModelInstances[i], new[] { backupValue });
                }
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            // Load the backup for all properties
            foreach (var property in ((ModelBase)ModelInstances[0]).GetDataProperties())
            {
                // Load the backup for the property
                LoadBackupForPropertyValue(property);
            }
            // Reinitialize the UI
            InitializeUI();
        }
    }
}