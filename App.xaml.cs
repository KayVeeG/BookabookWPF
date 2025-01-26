using System.Configuration;
using System.Data;
using System.Windows;

namespace BookabookWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database
            Globals.Database = new();
        }
    }

}
