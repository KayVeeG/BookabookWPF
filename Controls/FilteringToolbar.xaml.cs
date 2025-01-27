using BookabookWPF.Services;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookabookWPF.Controls
{

    public partial class FilteringToolbar : UserControl
    {

        public static readonly DependencyProperty ModelTypeProperty
            = DependencyProperty.Register(
                nameof(ModelType),
                typeof(Type),
                typeof(FilteringToolbar),
                new PropertyMetadata(null, OnModelTypeChanged));


        public Type ModelType
        {
            get => (Type)GetValue(ModelTypeProperty);
            set => SetValue(ModelTypeProperty, value);
        }
            
        public FilteringToolbar()
        {
            InitializeComponent();
        }

        private static void OnModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilteringToolbar filteringToolbar && e.NewValue != e.OldValue)
            {
                filteringToolbar.InitializeUI();
            }

        }


        protected void InitializeUI()
        {
            // Clear everything up
            Searchbar.Clear();
            FilterDropDownPanel.Children.Clear();
            BreadcrumbPanel.Children.Clear();

            // Check if model type is set
            if (ModelType is null)
                return;

            // Get all properties of the model type
            var properties = Activator.CreateInstance(ModelType)!.GetType().GetProperties();
            foreach (var property in properties)
            {
                FilterDropDown filterDropDown = new() { PropertyInfo = property };
                FilterDropDownPanel.Children.Add(filterDropDown);
            }
        }

    }
}
