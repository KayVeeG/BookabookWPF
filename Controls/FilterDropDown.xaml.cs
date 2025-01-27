using System.Windows.Controls;
using System.Reflection;
using System.Windows;
using BookabookWPF.Services;
using System.Windows.Data;

namespace BookabookWPF.Controls
{
    /// <summary>
    /// Interaktionslogik für FilterDropDown.xaml
    /// </summary>
    /// 

    public class MinMaxDateTimeData
    {
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
    }

    public partial class FilterDropDown : UserControl
    {

        // Dependency property for internal access
        public static readonly DependencyProperty PropertyInfoProperty =
            DependencyProperty.Register(nameof(PropertyInfo), typeof(PropertyInfo), typeof(FilterDropDown), new PropertyMetadata(null, OnPropertyInfoChanged));



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

            // Create DataTemplate
            DataTemplate template = new DataTemplate();

            // Create the factory for StackPanel
            FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType))
            {
                case TypeCode.DateTime:
                    // Create factories for DatePickers
                    FrameworkElementFactory minDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                    minDatePickerFactory.SetBinding(
                        DatePicker.SelectedDateProperty,
                        new Binding(nameof(MinMaxDateTimeData.MinDate)) { Mode = BindingMode.TwoWay }
                    );

                    FrameworkElementFactory maxDatePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                    maxDatePickerFactory.SetBinding(
                        DatePicker.SelectedDateProperty,
                        new Binding(nameof(MinMaxDateTimeData.MaxDate)) { Mode = BindingMode.TwoWay }
                    );

                    // Add DatePickers to StackPanel
                    stackPanelFactory.AppendChild(minDatePickerFactory);
                    stackPanelFactory.AppendChild(maxDatePickerFactory);

                    // Set the visual tree of the data template
                    template.VisualTree = stackPanelFactory;

                    // Assign the template to the ComboBox
                    ComboBox.ItemTemplate = template;

                    // Set the data context of the ComboBox
                    ComboBox.DataContext = new MinMaxDateTimeData();

                    break;
                default:
                    break;
            }


        }
    }

        
}
