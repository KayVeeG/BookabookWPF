using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookabookWPF.Controls
{
    public partial class Breadcrumb : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(Breadcrumb),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(
                nameof(CloseCommand),
                typeof(ICommand),
                typeof(Breadcrumb),
                new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public Breadcrumb()
        {
            InitializeComponent();
        }
    }
}