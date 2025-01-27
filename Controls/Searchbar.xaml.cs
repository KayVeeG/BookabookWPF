using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookabookWPF.Controls
{
    /// <summary>
    /// Interaktionslogik für Searchbar.xaml
    /// </summary>
    public partial class Searchbar : UserControl
    {

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText),
        typeof(string),
        typeof(Searchbar),
        new FrameworkPropertyMetadata(
        string.Empty,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        null,
        null,
        true,
        UpdateSourceTrigger.PropertyChanged)
        );

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }


        public static readonly RoutedEvent SearchClickedEvent = EventManager.RegisterRoutedEvent(
        name: nameof(SearchClicked),
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(Searchbar));



        // CLR event wrapper
        public event RoutedEventHandler SearchClicked
        {
            add { AddHandler(SearchClickedEvent, value); }
            remove { RemoveHandler(SearchClickedEvent, value); }
        }

        public Searchbar()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            SearchTextBox.Clear();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SearchClickedEvent));
        }
    }
}
