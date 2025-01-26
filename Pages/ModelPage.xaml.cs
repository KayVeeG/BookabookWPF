using System.Windows;
using System.Windows.Controls;
using BookabookWPF.Services;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Data;


namespace BookabookWPF.Pages
{
    /// <summary>
    /// Interaction logic for ModelPage.xaml
    /// </summary>
    public partial class ModelPage : Page
    {

        public static readonly DependencyProperty ModelTypeProperty = DependencyProperty.Register(
            nameof(ModelType),
            typeof(Type),
            typeof(ModelPage),
            new PropertyMetadata(null, OnModelTypeChanged)
        );

        public Type ModelType
        {
            get => (Type)GetValue(ModelTypeProperty);
            set => SetValue(ModelTypeProperty, value);
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
                // Make cell template
                var template = new DataTemplate();
                // Make display textbox for cell
                var textBlock = new FrameworkElementFactory(typeof(TextBlock));
                // Bind text property to item property
                textBlock.SetBinding(TextBlock.TextProperty, new Binding(property.Name));
                template.VisualTree = textBlock;

                // Add column
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = property.Name, // Header
                    CellTemplate = template, // Cell template
                    Width = 200 // Width
                });
            }



            // Create a observable collection and bind it to the list view
            observableCollection = new();
            modelView.ItemsSource = observableCollection;

            // Make method to get the items and invoke
            foreach(var item in (IEnumerable<object>)typeof(BookabookDatabase).GetMethod(nameof(BookabookDatabase.GetList))!.MakeGenericMethod(modelType)!.Invoke(Globals.Database, null)!)
            {
                observableCollection.Add(item);
            }


        }

        protected void EditItems(IList<object> items)
        {
            // Open item in edit window
            new Window()
            {
                Title = "Edit " + ModelType.Name,
                Content = new EditPage() { ModelInstances = items },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            }.ShowDialog();
            // Edit in database
            foreach (var item in items)
            {
                MethodInfo method = typeof(BookabookDatabase).GetMethod(nameof(BookabookDatabase.Update))!.MakeGenericMethod(item.GetType());
                method.Invoke(Globals.Database, new object[] { item });
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Create new item
            var item = Activator.CreateInstance(ModelType!);
            // Add to database
            Globals.Database!.Insert(item);
            // Add to model view
            observableCollection!.Add(item!);
            // Edit the item
            EditItems(new List<object>() { item! });
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
    }
}

