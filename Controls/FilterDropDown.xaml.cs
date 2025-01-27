using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using BookabookWPF.Services.Bookabook.Services;

namespace BookabookWPF.Controls
{
    public class MinMaxDateTimeData : INotifyPropertyChanged
    {
        private DateTime? minDate;
        private DateTime? maxDate;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected void InitializeUI()
        {
            // Clear everything up
            ComboBox.Items.Clear();

            // Set the Combobox header
            ComboBox.Text = PropertyInfo.Name;

            // Create DataTemplate
            DataTemplate template = new DataTemplate();

            // Create the factory for main StackPanel (vertical)
            FrameworkElementFactory mainStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            mainStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            // Create style for ComboBoxItems to prevent highlighting
            Style noHighlightStyle = new Style(typeof(ComboBoxItem));
            noHighlightStyle.Setters.Add(new Setter(ComboBoxItem.OverridesDefaultStyleProperty, true));
            noHighlightStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, SystemColors.WindowBrush));
            noHighlightStyle.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(ComboBoxItem))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            }));

            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType))
            {
                case TypeCode.DateTime:
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
                        // Handle any database errors gracefully
                        Debug.WriteLine($"Error fetching min/max dates: {ex.Message}");
                    }

                    // Create header with property name
                    FrameworkElementFactory headerTextFactory = new FrameworkElementFactory(typeof(TextBlock));
                    headerTextFactory.SetValue(TextBlock.TextProperty, PropertyInfo.Name);
                    headerTextFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 5));
                    mainStackPanelFactory.AppendChild(headerTextFactory);

                    // Create horizontal StackPanel for Min row
                    FrameworkElementFactory minRowStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                    minRowStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                    minRowStackPanelFactory.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 0, 5));

                    // Create Min label
                    FrameworkElementFactory minLabelFactory = new FrameworkElementFactory(typeof(TextBlock));
                    minLabelFactory.SetValue(TextBlock.TextProperty, "Min:");
                    minLabelFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    minLabelFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 5, 0));

                    // Create Min DatePicker
                    FrameworkElementFactory minDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                    var minBinding = new Binding(nameof(MinMaxDateTimeData.MinDate))
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    minDatePickerFactory.SetBinding(DatePicker.SelectedDateProperty, minBinding);

                    // Create horizontal StackPanel for Max row
                    FrameworkElementFactory maxRowStackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                    maxRowStackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                    // Create Max label
                    FrameworkElementFactory maxLabelFactory = new FrameworkElementFactory(typeof(TextBlock));
                    maxLabelFactory.SetValue(TextBlock.TextProperty, "Max:");
                    maxLabelFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    maxLabelFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 5, 0));

                    // Create Max DatePicker
                    FrameworkElementFactory maxDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                    var maxBinding = new Binding(nameof(MinMaxDateTimeData.MaxDate))
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    maxDatePickerFactory.SetBinding(DatePicker.SelectedDateProperty, maxBinding);

                    // Add Min/Max components to their respective stack panels
                    minRowStackPanelFactory.AppendChild(minLabelFactory);
                    minRowStackPanelFactory.AppendChild(minDatePickerFactory);
                    maxRowStackPanelFactory.AppendChild(maxLabelFactory);
                    maxRowStackPanelFactory.AppendChild(maxDatePickerFactory);

                    // Add both rows to the main vertical StackPanel
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
        }
    }
}