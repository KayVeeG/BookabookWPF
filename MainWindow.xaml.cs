using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BookabookWPF.Pages;

namespace BookabookWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeRuntimeUI();
        }

        private void InitializeRuntimeUI()
        {
            // Fill navigation list with model types
            foreach (var modelType in Utility.FindClassesWithAttribute<Attributes.ModelAttribute>())
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = modelType.Name;
                item.Tag = modelType;
                item.Selected += Item_Selected;
                NavigationList.Items.Add(item);
            }
        }

        private void Item_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                Type modelType = (Type)item.Tag;
                ModelPage modelPage = new();
                modelPage.ModelType = modelType;
                MainFrame.Navigate(modelPage);
            }
        }
    }

}