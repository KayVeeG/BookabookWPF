using BookabookWPF.Attributes;
using BookabookWPF.Controls;
using BookabookWPF.Converters;
using BookabookWPF.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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

        public static readonly DependencyProperty IsEditingNewItemProperty = 
            DependencyProperty.Register(
                nameof(IsEditingNewItem),
                typeof(bool),
                typeof(EditPage),
                new PropertyMetadata(false));


        public bool IsEditingNewItem
        {
            get => (bool)GetValue(IsEditingNewItemProperty);
            set => SetValue(IsEditingNewItemProperty, value);
        }

        // List that stores model instances backup for undo
        protected IList<object>? ModelInstancesBackup { get; set; }
        protected bool isLoaded = false;
        private IInputElement? lastFocusedElement = null;
        public EditPage()
        {
            Content = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Subscribe to window loaded event
            Loaded += (s, e) =>
            {
                isLoaded = true;
                if (ModelInstances is not null)
                {
                    InitializeUI();
                }

                var window = Window.GetWindow(this);
                if (window != null)
                {
                    // Track the last focused element before window starts closing
                    window.PreviewLostKeyboardFocus += (s, e) =>
                    {
                        lastFocusedElement = e.OldFocus;
                    };

                    window.Closing += (s, e) =>
                    {
                        // Store current focused element before validation
                        var currentFocused = FocusManager.GetFocusedElement(window);

                        // Validate all date pickers
                        foreach (StackPanel propertyControl in ((StackPanel)Content).Children.OfType<StackPanel>())
                        {
                            if (propertyControl != null && propertyControl.Children.OfType<DatePicker>().FirstOrDefault() is DatePicker datePicker)
                            {
                                // Store the next control in tab order
                                IInputElement nextControl = null;
                                datePicker.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                                nextControl = FocusManager.GetFocusedElement(window);

                                // Move focus back to date picker to trigger validation
                                datePicker.Focus();
                            }
                        }

                        // Check validation
                        bool isValid = ValidateModelInstances();

                        // If validation passes, restore focus to last focused element
                        if (isValid)
                        {
                            // If validation passes, restore focus to last focused element
                            if (lastFocusedElement != null && lastFocusedElement.IsEnabled)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    lastFocusedElement.Focus();
                                }), DispatcherPriority.Input);
                            }
                        }
                        else
                        {
                            // If validation fails, cancel window closing
                            e.Cancel = true;

                            // Restore original focus if validation failed
                            if (currentFocused != null && currentFocused.IsEnabled)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    currentFocused.Focus();
                                }), DispatcherPriority.Input);
                            }
                        }
                    };
                }
            };
        }
        protected bool ValidateModelInstances()
        {

            // Iterate through all model instances
            foreach (var instance in ModelInstances)
            {
                // Get the properties of the model instance
                var properties = ((ModelBase)instance).GetDataProperties();
                foreach (var property in properties)
                {
                    // Get the property value
                    object? value = property.GetValue(instance);

                    // Check if the property has MayNotBeNull attribute
                    if (property.GetCustomAttribute<MayNotBeNullAttribute>() != null)
                    {

                        // Check if the value is null
                        if (value is null)
                        {
                            // Get all the fields that are required
                            var requiredFields = properties.Where(p => p.GetCustomAttribute<MayNotBeNullAttribute>() != null).Select(p => p.Name);
                            // Build the message box message
                            StringBuilder stringBuilder = new();
                            stringBuilder.AppendLine("Please fill in all required fields:");
                            foreach (var field in requiredFields)
                            {
                                stringBuilder.AppendLine(field);
                            }
                            // Show message box
                            Xceed.Wpf.Toolkit.MessageBox.Show(stringBuilder.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            // Cancel the window closing
                            return false;
                        }
                    }
                    // Check if the property has RequiresValidation attribute
                    if (property.GetCustomAttribute<RequiresValidationAttribute>() != null)
                    {
                        // Invoke the validation method on the extracted object
                        bool isValid = property.GetCustomAttribute<RequiresValidationAttribute>()!.ValidationMethod.Invoke(value);
                        // If the validation fails
                        if (!isValid)
                        {
                            // Build the message box message
                            StringBuilder stringBuilder = new();
                            stringBuilder.AppendLine("Validation failed for " + property.Name);
                            // Check if there is a user instruction
                            if (property.GetCustomAttribute<RequiresValidationAttribute>()!.UserInstruction is not null)
                            {
                                stringBuilder.AppendLine(property.GetCustomAttribute<RequiresValidationAttribute>()!.UserInstruction);
                            }
                            // Show message box
                            Xceed.Wpf.Toolkit.MessageBox.Show(stringBuilder.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            // Cancel the window closing
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void OnModelInstancesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditPage editPage && e.NewValue != e.OldValue && e.NewValue is not null && editPage.isLoaded)
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

                // Manually set the model instance values to initial control value for syncing up
                //UpdatePropertyValues(property, GetControlValue(control));

                // Create checkbox for edit toggle
                CheckBox? checkBox = new()
                {
                    IsChecked = autoEnable,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = control
                };
                // Subscribe to checked changed event
                checkBox.Checked += OnCheckBoxCheckedChanged;
                checkBox.Unchecked += OnCheckBoxCheckedChanged;

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
                Margin = new Thickness(0, 10, 0, 0),
                Content = "HEHE",
                Width=200,
                Height=30
            };

            // Bind content of button to wether it's editing a new item or not
            undoButton.SetBinding(ContentProperty, new Binding(nameof(IsEditingNewItem))
            {
                Source = this,
                Converter = new BooleanToValueConverter<string>()
                {
                    TrueValue = "Load defaults",
                    FalseValue = "Undo all edits"
                },
            });

            // Subscribe to click event
            undoButton.Click += UndoButton_Click;

            // Add undo button to stack panel
            stackPanel.Children.Add(undoButton);

            // Make finish button
            var finishButton = new Button()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Content = "Finish",
                Width = 200,
                Height = 30
            };
            // Subscribe to click event
            finishButton.Click += (s, e) =>
            {
                Window.GetWindow(this)!.Close();
            };

            // Add finish button to stack panel
            stackPanel.Children.Add(finishButton);

            // Make cancel editing button if editing a new item
            if (IsEditingNewItem)
            {
                // Make cancel button
                var cancelButton = new Button
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Content = "Cancel",
                    Width = 200,
                    Height = 30
                };
                // Subscribe to click event
                cancelButton.Click += (s, e) =>
                {
                    // Remove the new item
                    ModelInstances.Clear();
                    // Close the window
                    Window.GetWindow(this)!.Close();
                };
                // Add cancel button to stack panel
                stackPanel.Children.Add(cancelButton);
            }



        }

        private FrameworkElement CreateControlForProperty(PropertyInfo property)
        {
            #region VariablePreperation
            // Abstract the property type
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // Extract the property value from first element
            object? value = property.GetValue(ModelInstances[0]);

            // Check if property is nullable
            bool isNullable = IsNullable(property);
            #endregion VariablePreperation

            #region TypeSwitching
            // Handle different property types
            switch (Type.GetTypeCode(propertyType))
            {
                case TypeCode.DateTime:
                    // Make date picker
                    var datePicker = new DatePicker()
                    {
                        SelectedDate = (DateTime?)value, // Set the value
                        Tag = property
                    };

                    // Subscribe to value changed event
                    datePicker.SelectedDateChanged += (s, e) =>
                    {
                        if (s is DatePicker datePicker && datePicker.Tag is PropertyInfo property)
                        {
                            // Update the property value
                            UpdatePropertyValues(property, datePicker.SelectedDate);
                        }
                    };

                    // Handle text input directly in the Loaded event
                    datePicker.Loaded += (s, e) =>
                    {
                        datePicker.ApplyTemplate();
                        if (datePicker.Template.FindName("PART_TextBox", datePicker) is DatePickerTextBox textBox)
                        {
                            textBox.TextChanged += (sender, args) =>
                            {
                                if (DateTime.TryParse(textBox.Text, out DateTime date))
                                {
                                    UpdatePropertyValues(property, date);
                                }
                            };
                        }
                    };



                    // Return the date picker
                    return datePicker;
                #region Decimal
                // Case of decimal value (such as price for example)
                case TypeCode.Decimal:
                    #region Decimal control generation
                    // Make double up down with third party library
                    var doubleUpDown = new Xceed.Wpf.Toolkit.DoubleUpDown
                    {
                        Value = value is null ? null : Convert.ToDouble(value), // Set the value
                        FormatString = "F2",  // Display 2 decimal places
                        Minimum = isNullable ? -0.01 : 0, // Set minimum value to -0.01 if nullable, 0 otherwise
                        Increment = 0.01 // Increment value of 1 cent
                    };

                    #endregion Decimal control generation

                    #region Decimal event subscription
                    // Subscribe to value changed event
                    
                    doubleUpDown.ValueChanged += (s, e) =>
                    {
                        if (s is Xceed.Wpf.Toolkit.DoubleUpDown nud && nud.Tag is PropertyInfo prop)
                        {
                            // Check if user wants to input null
                            if (isNullable && nud.Value < 0)
                            {
                                // Update the property value
                                nud.Value = null;
                            }

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
                    var foreignKeyAttr = property.GetCustomAttribute<ForeignKeyAttribute>();
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
                        Slider slider;

                        // Check if property is nullable
                        if (isNullable)
                        {
                            // Make nullable slider
                            slider = new NullableSlider();
                            // Set min/max before value
                            slider.Minimum = Convert.ToDouble(rangeAttr.Minimum);
                            slider.Maximum = Convert.ToDouble(rangeAttr.Maximum);
                            ((NullableSlider)slider).NullableValue = value is null ? null : Convert.ToDouble(value);
                        }
                        else
                        {
                            // Make regular slider
                            slider = new Slider();
                            // Set min/max before value
                            slider.Minimum = Convert.ToDouble(rangeAttr.Minimum);
                            slider.Maximum = Convert.ToDouble(rangeAttr.Maximum);
                            slider.Value = Convert.ToDouble(value);
                        }

                        // Set additional slider properties
                        slider.IsSnapToTickEnabled = true;
                        slider.TickFrequency = 1;


                        // Make value displaying label
                        TextBlock label = new();
                        // Bind the label text to slider value
                        /*label.SetBinding(TextBlock.TextProperty, new Binding()
                        {
                            Source = slider, // Source is the slider
                            Path = new PropertyPath(nameof(NullableSlider.NullableValue)), // Path is the value property of the slider
                            StringFormat = "{0:F0}", // Format the value to integer
                        });*/
                        // Set the grid column for label and slider
                        label.SetValue(Grid.ColumnProperty, 0);
                        slider.SetValue(Grid.ColumnProperty, 1);

                        // Add controls to grid
                        grid.Children.Add(label);
                        grid.Children.Add(slider);
                        #endregion Int32 Range control generation

                        #region Int32 Range event subscription
                        // Subscribe to value changed event
                        if (slider is NullableSlider nullableSlider)
                        {
                            // Set the initial value before binding
                            nullableSlider.NullableValue = value is null ? null : Convert.ToDouble(value);

                            nullableSlider.NullableValueChanged += (s, e) =>
                            {
                                if (((FrameworkElement)nullableSlider.Parent).Tag is PropertyInfo property)
                                {
                                    UpdatePropertyValues(property, e.NewValue is null ? null : Convert.ToInt32(e.NewValue));
                                }
                            };
                        }
                        else
                        {
                            slider.ValueChanged += (s, e) =>
                            {
                                if (((FrameworkElement)slider.Parent).Tag is PropertyInfo property)
                                {
                                    // Update the property value with non-null value
                                    UpdatePropertyValues(property, Convert.ToInt32(e.NewValue));
                                }
                            };
                        }
                        #endregion Int32 Range event subscription

                        #region Int32 Range return
                        // Return the grid
                        return grid;
                        #endregion Int32 Range return
                    }
                    #endregion Int32 RangeAttribute check
                    #region Int32 ForeignKey check
                    // Check for ForeignKey attribute
                    else if (foreignKeyAttr != null)
                    {
                        #region Int32 ForeignKey control generation

                        // Make a grid containing the following buttons
                        Grid grid = new()
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Auto)},
                                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Auto)}
                            }
                        };

                        // Make button to choose foreign model instance
                        Button chooseButton = new()
                        {
                            Content = "Choose" // Set the content
                        };

                        // Check if MayNotBeNull attribute is present
                        Button? unchooseButton = null;
                        if (property.GetCustomAttribute<MayNotBeNullAttribute>() != null)
                        {
                            // Create a unchoose button
                            unchooseButton = new()
                            {
                                Content = "Unchoose" // Set the content
                            };
                        }
                        #endregion Int32 ForeignKey control generation
                        #region Int32 ForeignKey event subscription
                        // Subscribe to choose button click event
                        chooseButton.Click += (s, e) =>
                        {
                            // Create a new instance of the foreign model page
                            ModelPage modelPage = (ModelPage)Activator.CreateInstance(typeof(ModelPage))!;

                            // Set mode to choosing since it's a foreign key
                            modelPage.SetValue(ModelPage.ChoosingEnabledProperty, true);

                            // Set the model type of the model page
                            modelPage.SetValue(ModelPage.ModelTypeProperty, Type.GetType(foreignKeyAttr.Name)!);

                            // Create new window in which the page gets added
                            new Window()
                            {
                                Title = "Choose " + modelPage.ModelType.Name, // Set the title of the window
                                Content = modelPage, // Set the content of the window to the model page
                                SizeToContent = SizeToContent.WidthAndHeight, // Size to content
                                ResizeMode = ResizeMode.CanResize, // Can resize
                                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner, // Center the window
                                Owner = Application.Current.MainWindow, // Set the owner of the window
                                WindowStyle = WindowStyle.ToolWindow    // Set the window style to tool window
                            }.ShowDialog();

                            // Get the chosen foreign model instance
                            ModelBase? foreignModel = (ModelBase?)modelPage.ChosenModelInstance;
                            // Get the primary key value of the foreign model instance
                            object? primaryKeyValue = foreignModel?.GetPrimaryKeyValue();
                            // Update the property value
                            UpdatePropertyValues(property, primaryKeyValue);

                        };
                        // Check if unchoose button is available
                        if (unchooseButton is not null)
                        {
                            // Subscribe to unchoose button click event
                            unchooseButton.Click += (s, e) =>
                            {
                                // Update the property value to null
                                UpdatePropertyValues(property, null);
                            };
                        }

                        #endregion Int32 ForeignKey event subscription
                        #region Int32 ForeignKey return
                        // Set button grid layout and add to grid
                        chooseButton.SetValue(Grid.ColumnProperty, 0);
                        grid.Children.Add(chooseButton);
                        if (unchooseButton is not null)
                        {
                            unchooseButton.SetValue(Grid.ColumnProperty, 1);
                            grid.Children.Add(unchooseButton);
                        }
                        // Return the grid that contains the buttons
                        return grid;
                        #endregion Int32 ForeignKey return
                    }
                    #endregion Int32 ForeignKey check
                    else
                    {
                        goto default;
                    }

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
                    object? firstBackupValue = property.GetValue(ModelInstancesBackup![0]);
                    // Set the control value to the first backup value
                    SetControlValue(control, firstBackupValue);
                    // Load the backup for the property values independently from the first
                    LoadBackupForPropertyValue(property);
                }
            }
        }

        private object? GetControlValue(FrameworkElement control)
        {
            // Check if grid is slider grid
            if (control is Grid grid)
            {
                // Get the slider control
                Slider slider = grid.Children.OfType<Slider>().First();

                // Check if slider is nullable
                if (slider is NullableSlider nullableSlider)
                {
                    // Get the nullable value of the nullable slider
                    object? nullableValue = nullableSlider.NullableValue;
                    // Return the nullable value
                    return nullableValue is null ? null : Convert.ToInt32(nullableValue);
                }
                else if (slider is Slider)
                {
                    // Return the value of the slider
                    return Convert.ToInt32(slider.Value);
                }
            }

            return control switch
            {
                TextBox textBox => textBox.Text,
                ComboBox comboBox => comboBox.Text,
                DatePicker datePicker => datePicker.SelectedDate,
                Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown => (decimal?)numericUpDown.Value,
                _ => null
            };
        }

        private void SetControlValue(FrameworkElement control, object? value)
        {
            // Check if grid is slider grid
            if (control is Grid grid) {
                // Get the slider control
                Slider slider = grid.Children.OfType<Slider>().First();
                // Check if slider is nullable
                if (slider is NullableSlider nullableSlider)
                {
                    // Set the nullable value of the nullable slider
                    nullableSlider.NullableValue = value is null ? null : Convert.ToDouble(value);
                }
                else if (slider is Slider)
                {
                    // Set the value of the slider
                    slider.Value = Convert.ToDouble(value);
                }
            }

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
                case Xceed.Wpf.Toolkit.DoubleUpDown numericUpDown:
                    numericUpDown.Value = Convert.ToDouble(value);
                    break;
            }
        }

        private static bool IsNullable(PropertyInfo property)
        {
            var nullabilityContext = new NullabilityInfoContext();
            var nullabilityInfo = nullabilityContext.Create(property);
            return nullabilityInfo.WriteState == NullabilityState.Nullable ||
                   nullabilityInfo.ReadState == NullabilityState.Nullable;
        }

        protected void UpdatePropertyValues(PropertyInfo property, object? value)
        {
            // Iterate through all model instances
            foreach (var instance in ModelInstances)
            {
                // Get the property type
                Type propertyType = property.PropertyType;
                // Get the setter method
                MethodInfo? setter = property.GetSetMethod();
                // Check if the property is nullable and the value is an empty string
                if (IsNullable(property))
                {
                    if (value is string str && str == string.Empty)
                    {
                        value = null;
                    }
                }
    

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