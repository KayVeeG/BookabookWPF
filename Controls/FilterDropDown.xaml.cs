using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using BookabookWPF.Services.Bookabook.Services;
using System.Collections.ObjectModel;
using BookabookWPF.Attributes;
using BookabookWPF.Services;

namespace BookabookWPF.Controls
{
    public class MinMaxDecimalData : INotifyPropertyChanged
    {
        private decimal? minValue;
        private decimal? maxValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public decimal? MinValue
        {
            get => minValue;
            set
            {
                if (minValue != value)
                {
                    minValue = value;
                    OnPropertyChanged(nameof(MinValue));

                    // If MaxValue is less than MinValue, update it
                    if (maxValue.HasValue && minValue.HasValue && maxValue < minValue)
                    {
                        MaxValue = minValue;
                    }
                }
            }
        }

        public decimal? MaxValue
        {
            get => maxValue;
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                    OnPropertyChanged(nameof(MaxValue));

                    // If MinValue is greater than MaxValue, update it
                    if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
                    {
                        MinValue = maxValue;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class StringMultipleSelectionData : INotifyPropertyChanged
    {
        private HashSet<string> selectedValues = new();
        private ObservableCollection<MultipleSelectionItem> availableValues = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MultipleSelectionItem> AvailableValues
        {
            get => availableValues;
            set
            {
                availableValues = value;
                OnPropertyChanged(nameof(AvailableValues));
            }
        }

        public HashSet<string> SelectedValues
        {
            get => selectedValues;
            set
            {
                selectedValues = value;
                OnPropertyChanged(nameof(SelectedValues));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MultipleSelectionItem : INotifyPropertyChanged
    {
        private bool isSelected;
        private string value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class MinMaxInt32Data : INotifyPropertyChanged
    {
        private int? minValue;
        private int? maxValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public int? MinValue
        {
            get => minValue;
            set
            {
                if (minValue != value)
                {
                    minValue = value;
                    OnPropertyChanged(nameof(MinValue));

                    // If MaxValue is less than MinValue, update it
                    if (maxValue.HasValue && minValue.HasValue && maxValue < minValue)
                    {
                        MaxValue = minValue;
                    }
                }
            }
        }

        public int? MaxValue
        {
            get => maxValue;
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                    OnPropertyChanged(nameof(MaxValue));

                    // If MinValue is greater than MaxValue, update it
                    if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
                    {
                        MinValue = maxValue;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MinMaxDateTimeData : INotifyPropertyChanged
    {
        private DateTime? minDate;
        private DateTime? maxDate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime? MinDate
        {
            get => minDate;
            set
            {
                if (minDate != value)
                {
                    minDate = value;
                    OnPropertyChanged(nameof(MinDate));

                    // If MaxDate is less than MinDate, update it
                    if (maxDate.HasValue && minDate.HasValue && maxDate < minDate)
                    {
                        MaxDate = minDate;
                    }
                }
            }
        }

        public DateTime? MaxDate
        {
            get => maxDate;
            set
            {
                if (maxDate != value)
                {
                    maxDate = value;
                    OnPropertyChanged(nameof(MaxDate));

                    // If MinDate is greater than MaxDate, update it
                    if (minDate.HasValue && maxDate.HasValue && minDate > maxDate)
                    {
                        MinDate = maxDate;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class FilterDropDown : UserControl
    {
        public event EventHandler<Filter>? FilterChanged;

        // Dependency property for internal access
        public static readonly DependencyProperty PropertyInfoProperty =
            DependencyProperty.Register(nameof(PropertyInfo), typeof(PropertyInfo), typeof(FilterDropDown),
                new PropertyMetadata(null, OnPropertyInfoChanged));

        // Public property for external access
        public PropertyInfo PropertyInfo
        {
            get => (PropertyInfo)GetValue(PropertyInfoProperty);
            set => SetValue(PropertyInfoProperty, value);
        }

        public FilterDropDown()
        {
            InitializeComponent();
        }

        private static void OnPropertyInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilterDropDown filterDropDown && e.NewValue != e.OldValue)
            {
                filterDropDown.InitializeUI();
            }
        }

        private void OnNumberValueChanged(MinMaxInt32Data data)
        {
            if (data.MinValue.HasValue || data.MaxValue.HasValue)
            {
                var conditions = new List<string>();
                if (data.MinValue.HasValue)
                    conditions.Add($"{PropertyInfo.Name} >= {data.MinValue.Value}");
                if (data.MaxValue.HasValue)
                    conditions.Add($"{PropertyInfo.Name} <= {data.MaxValue.Value}");

                var filter = new Filter(PropertyInfo, string.Join(" AND ", conditions));
                FilterChanged?.Invoke(this, filter);
            }
        }

        private void OnDecimalValueChanged(MinMaxDecimalData data)
        {
            if (data.MinValue.HasValue || data.MaxValue.HasValue)
            {
                var conditions = new List<string>();
                if (data.MinValue.HasValue)
                    conditions.Add($"{PropertyInfo.Name} >= {data.MinValue.Value}");
                if (data.MaxValue.HasValue)
                    conditions.Add($"{PropertyInfo.Name} <= {data.MaxValue.Value}");

                var filter = new Filter(PropertyInfo, string.Join(" AND ", conditions));
                FilterChanged?.Invoke(this, filter);
            }
        }

        private void OnDateValueChanged(MinMaxDateTimeData data)
        {
            if (data.MinDate.HasValue || data.MaxDate.HasValue)
            {
                var conditions = new List<string>();
                if (data.MinDate.HasValue)
                    conditions.Add($"{PropertyInfo.Name} >= datetime('{data.MinDate.Value:yyyy-MM-dd}')");
                if (data.MaxDate.HasValue)
                    conditions.Add($"{PropertyInfo.Name} <= datetime('{data.MaxDate.Value:yyyy-MM-dd}')");

                var filter = new Filter(PropertyInfo, string.Join(" AND ", conditions));
                FilterChanged?.Invoke(this, filter);
            }
        }

        private void OnMultipleSelectionChanged(StringMultipleSelectionData data)
        {
            var selectedValues = data.AvailableValues.Where(x => x.IsSelected).Select(x => x.Value).ToList();
            if (selectedValues.Any())
            {
                var valuesList = string.Join(",", selectedValues.Select(v => $"'{v}'"));
                var filter = new Filter(PropertyInfo, $"{PropertyInfo.Name} IN ({valuesList})");
                FilterChanged?.Invoke(this, filter);
            }
        }

        public void InitializeUI()
        {
            // Clear everything up
            ComboBox.ItemsSource = null;

            // Set the Combobox header
            ComboBox.Text = PropertyInfo.Name;

            // Create DataTemplate
            DataTemplate template = new DataTemplate();

            // Create the factory for main StackPanel (vertical)
            FrameworkElementFactory mainStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            mainStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            // Create common factories outside switch
            FrameworkElementFactory headerTextFactory = new FrameworkElementFactory(typeof(TextBlock));
            FrameworkElementFactory minRowStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory maxRowStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory minLabelFactory = new FrameworkElementFactory(typeof(TextBlock));
            FrameworkElementFactory maxLabelFactory = new FrameworkElementFactory(typeof(TextBlock));

            // Set common properties
            headerTextFactory.SetValue(TextBlock.TextProperty, PropertyInfo.Name);
            headerTextFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 5));

            minRowStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            minRowStackPanelFactory.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 0, 5));

            maxRowStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            minLabelFactory.SetValue(TextBlock.TextProperty, "Min:");
            minLabelFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            minLabelFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 5, 0));

            maxLabelFactory.SetValue(TextBlock.TextProperty, "Max:");
            maxLabelFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            maxLabelFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 5, 0));

            // Create style for ComboBoxItems to prevent highlighting
            Style noHighlightStyle = new Style(typeof(ComboBoxItem));
            noHighlightStyle.Setters.Add(new Setter(ComboBoxItem.OverridesDefaultStyleProperty, true));
            noHighlightStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, SystemColors.WindowBrush));
            noHighlightStyle.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(ComboBoxItem))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            }));

            var typeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Int32:
                    {
                        // Initialize data with database values
                        var int32Data = new MinMaxInt32Data();
                        try
                        {
                            string tableName = PropertyInfo.DeclaringType!.Name;
                            string columnName = PropertyInfo.Name;

                            // Get min and max values from database
                            int32Data.MinValue = Globals.Database!.GetMinValue<int?>(tableName, columnName);
                            int32Data.MaxValue = Globals.Database!.GetMaxValue<int?>(tableName, columnName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error fetching min/max values: {ex.Message}");
                        }

                        // Get range attribute for control limits
                        var rangeAttribute = PropertyInfo.GetCustomAttribute<RangeAttribute>();
                        double minLimit = rangeAttribute != null ? (double)(int)rangeAttribute.Minimum : int.MinValue;
                        double maxLimit = rangeAttribute != null ? (double)(int)rangeAttribute.Maximum : int.MaxValue;

                        // Create Min DoubleUpDown
                        var minUpDownFactory = new FrameworkElementFactory(typeof(Xceed.Wpf.Toolkit.DoubleUpDown));
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.FormatStringProperty, "F0");
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MinimumProperty, minLimit);
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MaximumProperty, maxLimit);

                        // Add value changed handler
                        minUpDownFactory.AddHandler(
                            Xceed.Wpf.Toolkit.DoubleUpDown.ValueChangedEvent,
                            new RoutedPropertyChangedEventHandler<object?>((s, e) => OnNumberValueChanged(int32Data)));

                        var minBinding = new Binding(nameof(MinMaxInt32Data.MinValue))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        minUpDownFactory.SetBinding(Xceed.Wpf.Toolkit.DoubleUpDown.ValueProperty, minBinding);

                        // Create Max DoubleUpDown
                        var maxUpDownFactory = new FrameworkElementFactory(typeof(Xceed.Wpf.Toolkit.DoubleUpDown));
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.FormatStringProperty, "F0");
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MinimumProperty, minLimit);
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MaximumProperty, maxLimit);

                        // Add value changed handler
                        maxUpDownFactory.AddHandler(
                            Xceed.Wpf.Toolkit.DoubleUpDown.ValueChangedEvent,
                            new RoutedPropertyChangedEventHandler<object?>((s, e) => OnNumberValueChanged(int32Data)));

                        var maxBinding = new Binding(nameof(MinMaxInt32Data.MaxValue))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        maxUpDownFactory.SetBinding(Xceed.Wpf.Toolkit.DoubleUpDown.ValueProperty, maxBinding);

                        // Assemble the control structure
                        minRowStackPanelFactory.AppendChild(minLabelFactory);
                        minRowStackPanelFactory.AppendChild(minUpDownFactory);
                        maxRowStackPanelFactory.AppendChild(maxLabelFactory);
                        maxRowStackPanelFactory.AppendChild(maxUpDownFactory);

                        mainStackPanelFactory.AppendChild(headerTextFactory);
                        mainStackPanelFactory.AppendChild(minRowStackPanelFactory);
                        mainStackPanelFactory.AppendChild(maxRowStackPanelFactory);

                        // Set the visual tree of the data template
                        template.VisualTree = mainStackPanelFactory;

                        // Assign the template and data context
                        ComboBox.ItemTemplate = template;
                        ComboBox.ItemsSource = new[] { int32Data };
                        ComboBox.ItemContainerStyle = noHighlightStyle;
                        break;
                    }

                case TypeCode.DateTime:
                    {
                        // Initialize data with database values
                        var dateTimeData = new MinMaxDateTimeData();
                        try
                        {
                            string tableName = PropertyInfo.DeclaringType!.Name;
                            string columnName = PropertyInfo.Name;

                            // Get min and max values from database
                            dateTimeData.MinDate = Globals.Database!.GetMinValue<DateTime?>(tableName, columnName);
                            dateTimeData.MaxDate = Globals.Database!.GetMaxValue<DateTime?>(tableName, columnName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error fetching min/max dates: {ex.Message}");
                        }

                        // Create Min DatePicker
                        var minDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                        var minDateBinding = new Binding(nameof(MinMaxDateTimeData.MinDate))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        minDatePickerFactory.SetBinding(DatePicker.SelectedDateProperty, minDateBinding);

                        // Create Max DatePicker
                        var maxDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                        var maxDateBinding = new Binding(nameof(MinMaxDateTimeData.MaxDate))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        maxDatePickerFactory.SetBinding(DatePicker.SelectedDateProperty, maxDateBinding);

                        // Assemble the control structure
                        minRowStackPanelFactory.AppendChild(minLabelFactory);
                        minRowStackPanelFactory.AppendChild(minDatePickerFactory);
                        maxRowStackPanelFactory.AppendChild(maxLabelFactory);
                        maxRowStackPanelFactory.AppendChild(maxDatePickerFactory);

                        mainStackPanelFactory.AppendChild(headerTextFactory);
                        mainStackPanelFactory.AppendChild(minRowStackPanelFactory);
                        mainStackPanelFactory.AppendChild(maxRowStackPanelFactory);

                        // Set the visual tree of the data template
                        template.VisualTree = mainStackPanelFactory;

                        // Assign the template and data context
                        ComboBox.ItemTemplate = template;
                        ComboBox.ItemsSource = new[] { dateTimeData };
                        ComboBox.ItemContainerStyle = noHighlightStyle;
                        break;

                    }
                case TypeCode.String:
                    {
                        // Check if the property has MultipleInDatabase attribute
                        var multipleAttribute = PropertyInfo.GetCustomAttribute<MultipleInDatabaseAttribute>();
                        if (multipleAttribute != null)
                        {
                            // Initialize data
                            var multipleData = new StringMultipleSelectionData();
                            try
                            {
                                string tableName = PropertyInfo.DeclaringType!.Name;
                                string columnName = PropertyInfo.Name;

                                // Get distinct values from database - method always returns strings
                                var distinctValues = Globals.Database!.GetDistinctValues(tableName, columnName);

                                // Create selection items
                                multipleData.AvailableValues = new ObservableCollection<MultipleSelectionItem>(
                                    distinctValues.Select(value => new MultipleSelectionItem
                                    {
                                        Value = value,
                                        IsSelected = false
                                    })
                                );
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error fetching distinct values: {ex.Message}");
                            }

                            // Create ItemsControl for checkboxes
                            var itemsControlFactory = new FrameworkElementFactory(typeof(ItemsControl));
                            itemsControlFactory.SetValue(ItemsControl.MarginProperty, new Thickness(5));

                            // Create ItemTemplate for the ItemsControl
                            var checkBoxTemplate = new DataTemplate();
                            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
                            checkBoxFactory.SetValue(CheckBox.MarginProperty, new Thickness(0, 2, 0, 2));
                            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(MultipleSelectionItem.IsSelected))
                            {
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                            });
                            checkBoxFactory.SetBinding(CheckBox.ContentProperty, new Binding(nameof(MultipleSelectionItem.Value)));
                            checkBoxTemplate.VisualTree = checkBoxFactory;

                            itemsControlFactory.SetValue(ItemsControl.ItemTemplateProperty, checkBoxTemplate);
                            itemsControlFactory.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(StringMultipleSelectionData.AvailableValues)));

                            // Create ScrollViewer to contain the ItemsControl
                            var scrollViewerFactory = new FrameworkElementFactory(typeof(ScrollViewer));
                            scrollViewerFactory.SetValue(ScrollViewer.MaxHeightProperty, 200.0);
                            scrollViewerFactory.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                            scrollViewerFactory.AppendChild(itemsControlFactory);

                            // Assemble the control structure
                            mainStackPanelFactory.AppendChild(headerTextFactory);
                            mainStackPanelFactory.AppendChild(scrollViewerFactory);

                            // Set the visual tree of the data template
                            template.VisualTree = mainStackPanelFactory;

                            // Assign the template and data context
                            ComboBox.ItemTemplate = template;
                            ComboBox.ItemsSource = new[] { multipleData };
                            ComboBox.ItemContainerStyle = noHighlightStyle;
                        }
                        break;
                    }
                case TypeCode.Decimal:
                    {
                        // Initialize data with database values
                        var decimalData = new MinMaxDecimalData();
                        try
                        {
                            string tableName = PropertyInfo.DeclaringType!.Name;
                            string columnName = PropertyInfo.Name;

                            // Get min and max values from database
                            decimalData.MinValue = Globals.Database!.GetMinValue<decimal?>(tableName, columnName);
                            decimalData.MaxValue = Globals.Database!.GetMaxValue<decimal?>(tableName, columnName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error fetching min/max values: {ex.Message}");
                        }

                        // Check for MustBePositive attribute and set minimum value accordingly
                        var mustBePositiveAttr = PropertyInfo.GetCustomAttribute<MustBePositiveAttribute>();
                        double minLimit = mustBePositiveAttr != null ? 0 : (double)decimal.MinValue;

                        // Create Min DoubleUpDown
                        var minUpDownFactory = new FrameworkElementFactory(typeof(Xceed.Wpf.Toolkit.DoubleUpDown));
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.FormatStringProperty, "F2"); // Show 2 decimal places
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MinimumProperty, minLimit);
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MaximumProperty, (double)decimal.MaxValue);
                        minUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.IncrementProperty, 0.01); // Increment by 0.01

                        var minBinding = new Binding(nameof(MinMaxDecimalData.MinValue))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        minUpDownFactory.SetBinding(Xceed.Wpf.Toolkit.DoubleUpDown.ValueProperty, minBinding);

                        // Create Max DoubleUpDown
                        var maxUpDownFactory = new FrameworkElementFactory(typeof(Xceed.Wpf.Toolkit.DoubleUpDown));
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.FormatStringProperty, "F2"); // Show 2 decimal places
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MinimumProperty, minLimit);
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.MaximumProperty, (double)decimal.MaxValue);
                        maxUpDownFactory.SetValue(Xceed.Wpf.Toolkit.DoubleUpDown.IncrementProperty, 0.01); // Increment by 0.01

                        var maxBinding = new Binding(nameof(MinMaxDecimalData.MaxValue))
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        maxUpDownFactory.SetBinding(Xceed.Wpf.Toolkit.DoubleUpDown.ValueProperty, maxBinding);

                        // Assemble the control structure
                        minRowStackPanelFactory.AppendChild(minLabelFactory);
                        minRowStackPanelFactory.AppendChild(minUpDownFactory);
                        maxRowStackPanelFactory.AppendChild(maxLabelFactory);
                        maxRowStackPanelFactory.AppendChild(maxUpDownFactory);

                        mainStackPanelFactory.AppendChild(headerTextFactory);
                        mainStackPanelFactory.AppendChild(minRowStackPanelFactory);
                        mainStackPanelFactory.AppendChild(maxRowStackPanelFactory);

                        // Set the visual tree of the data template
                        template.VisualTree = mainStackPanelFactory;

                        // Assign the template and data context
                        ComboBox.ItemTemplate = template;
                        ComboBox.ItemsSource = new[] { decimalData };
                        ComboBox.ItemContainerStyle = noHighlightStyle;
                        break;
                    }
            }

        }
        
    }
}