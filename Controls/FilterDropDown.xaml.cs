using System.Windows.Controls;
using System.Reflection;
using System.Windows;
using BookabookWPF.Services;
using System.Windows.Data;
using System.Windows.Media;

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
                    minDatePickerFactory.SetBinding(
                        DatePicker.SelectedDateProperty,
                        new Binding(nameof(MinMaxDateTimeData.MinDate)) { Mode = BindingMode.TwoWay }
                    );

                    // Add label and DatePicker to the horizontal StackPanel
                    minRowStackPanelFactory.AppendChild(minLabelFactory);
                    minRowStackPanelFactory.AppendChild(minDatePickerFactory);

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
                    maxDatePickerFactory.SetBinding(
                        DatePicker.SelectedDateProperty,
                        new Binding(nameof(MinMaxDateTimeData.MaxDate)) { Mode = BindingMode.TwoWay }
                    );

                    // Add label and DatePicker to the horizontal StackPanel
                    maxRowStackPanelFactory.AppendChild(maxLabelFactory);
                    maxRowStackPanelFactory.AppendChild(maxDatePickerFactory);

                    // Add both rows to the main vertical StackPanel
                    mainStackPanelFactory.AppendChild(minRowStackPanelFactory);
                    mainStackPanelFactory.AppendChild(maxRowStackPanelFactory);

                    // Set the visual tree of the data template
                    template.VisualTree = mainStackPanelFactory;
                    // Assign the template to the ComboBox
                    ComboBox.ItemTemplate = template;
                    // Set the data context of the ComboBox
                    ComboBox.ItemsSource = new MinMaxDateTimeData[] { new MinMaxDateTimeData() };
                    // Apply the no-highlight style
                    ComboBox.ItemContainerStyle = noHighlightStyle;
                    break;
                default:
                    break;
            }
        }   
    }
}
        
