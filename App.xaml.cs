using System.Configuration;
using System.Data;
using System.Windows;
using BookabookWPF.Models;

namespace BookabookWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database
            Globals.Database = new();

            // Generate dummy data if the database is empty
            if (!Globals.Database.GetList<BookClass>().Any())
            {
                DummyDataGenerator.GenerateDummyData();
            }
        }
    }

}
