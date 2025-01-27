using System.Windows;
using System.Windows.Controls;
using BookabookWPF.Services;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading.Channels;
using System.Runtime.InteropServices;


namespace BookabookWPF.Pages
{
    /// <summary>
    /// Interaction logic for ModelPage.xaml
    /// </summary>
    public partial class ModelPage : Page
    {

        // Dependency property for internal access
        internal static readonly DependencyPropertyKey ChosenModelInstancePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ChosenModelInstance),
                typeof(ModelPage),
                typeof(ModelPage),
                new PropertyMetadata(null));


        // Public property for external access
        public static readonly DependencyProperty ChosenModelInstanceProperty =
            ChosenModelInstancePropertyKey.DependencyProperty;

        public static readonly DependencyProperty ModelTypeProperty = DependencyProperty.Register(
            nameof(ModelType),
            typeof(Type),
            typeof(ModelPage),
            new PropertyMetadata(null, OnModelTypeChanged)
        );

        public static readonly DependencyProperty ChoosingEnabledProperty = DependencyProperty.Register(
            nameof(ChoosingEnabled),
            typeof(bool),
            typeof(ModelPage),
            new PropertyMetadata(false)
        );

        public Type ModelType
        {
            get => (Type)GetValue(ModelTypeProperty);
            set => SetValue(ModelTypeProperty, value);
        }

        public bool ChoosingEnabled
        {
            get => (bool)GetValue(ChoosingEnabledProperty);
            set => SetValue(ChoosingEnabledProperty, value);
        }

        public object? ChosenModelInstance
        {
            get => modelView.SelectedItem;
        }

        protected ObservableCollection<object>? observableCollection;

        public ModelPage()
        {
            InitializeComponent();
        }

        private static void OnModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModelPage modelPage && e.NewValue != e.OldValue && e.NewValue is Type modelType)
            {
                modelPage.InitializeModelView(modelType);
            }
        }

        protected void InitializeModelView(Type modelType)
        {
            // Get each property of model type
            var properties = ((ModelBase)Activator.CreateInstance(modelType)!).GetDataProperties();

            // Assign the grid view
            GridView gridView = new();
            modelView.View = gridView;

            // Add columns with templates
            foreach (PropertyInfo property in properties)
            {
                // Create cell template
                var template = new DataTemplate();
                var factory = new FrameworkElementFactory(typeof(TextBlock));

                // Set text binding
                factory.SetBinding(TextBlock.TextProperty, new Binding(property.Name)
                {
                    TargetNullValue = "NULL"
                });

                // Add STYLING to match application theme
                factory.SetValue(TextBlock.PaddingProperty, new Thickness(10, 8, 10, 8));
                factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                factory.SetValue(TextBlock.FontSizeProperty, 14.0); // Match application font size

                template.VisualTree = factory;

                // Create header template
                var headerTemplate = new DataTemplate();
                var headerFactory = new FrameworkElementFactory(typeof(TextBlock));
                headerFactory.SetValue(TextBlock.TextProperty, property.Name);
                headerFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                headerFactory.SetValue(TextBlock.FontSizeProperty, 14.0); // Match application font size
                headerTemplate.VisualTree = headerFactory;

                // Add column with styling
                var column = new GridViewColumn
                {
                    HeaderTemplate = headerTemplate,
                    CellTemplate = template,
                    Width = 200
                };

                gridView.Columns.Add(column);
            }

            // Create observable collection and bind it to the list view
            observableCollection = new();
            modelView.ItemsSource = observableCollection;

            // Load data from database
            foreach (var item in (IEnumerable<object>)typeof(BookabookDatabase)
                .GetMethod(nameof(BookabookDatabase.GetList))!
                .MakeGenericMethod(modelType)!
                .Invoke(Globals.Database, null)!)
            {
                observableCollection.Add(item);
            }
        }

        protected void EditItems(IList<object> items, bool addInstead = false)
        {

            // Backup the items that are being edited (for late removal if adding is aborted)
            var itemsCopy = new List<object>(items);

            // Open item in edit window
            new Window()
            {
                Title = "Edit " + ModelType.Name, // Title
                Content = new EditPage() { ModelInstances = items, IsEditingNewItem = addInstead }, // Content
                WindowStartupLocation = WindowStartupLocation.CenterOwner, // Startup location
                ResizeMode = ResizeMode.NoResize, // No resize
                SizeToContent = SizeToContent.WidthAndHeight, // Size to content
                Owner = Application.Current.MainWindow, // Owner
                WindowStyle = WindowStyle.ToolWindow // Tool window
            }.ShowDialog();

            // If adding was not aborted
            if (items.Count != 0)
            {
                // Edit in / Add to database
                foreach (var item in items)
                {
                    if (!addInstead)
                    {
                        MethodInfo method = typeof(BookabookDatabase).GetMethod(nameof(BookabookDatabase.Update))!.MakeGenericMethod(item.GetType());
                        method.Invoke(Globals.Database, new object[] { item });
                    }
                    else
                    {
                        MethodInfo method = typeof(BookabookDatabase).GetMethod(nameof(BookabookDatabase.Insert))!.MakeGenericMethod(item.GetType());
                        method.Invoke(Globals.Database, new object[] { item });
                    }
                }
            }
            // If adding was aborted
            else
            {
                foreach (var item in itemsCopy)
                {
                    observableCollection!.Remove(item);
                }
            }

        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Create new item
            var item = Activator.CreateInstance(ModelType!);
            // Add to model view
            observableCollection!.Add(item!);
            // Edit the item
            EditItems(new List<object>() { item! }, true);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item
            var items = modelView.SelectedItems;
            if (items.Count > 0)
            {
                // Edit the items
                EditItems(items.Cast<object>().ToList());
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // Create a separate list of items to delete
            var itemsToDelete = modelView.SelectedItems.Cast<object>().ToArray();

            foreach (var selectedItem in itemsToDelete)
            {
                // Delete the item in database
                MethodInfo method = typeof(BookabookDatabase).GetMethod(nameof(BookabookDatabase.Delete))!.MakeGenericMethod(selectedItem.GetType());
                method.Invoke(Globals.Database, new object[] { selectedItem });
                // Delete the item in model view
                observableCollection!.Remove(selectedItem);
            }

        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            // Get the window
            Window window = Window.GetWindow(this);

            // Close the window
            window?.Close();
        }

    }
}

