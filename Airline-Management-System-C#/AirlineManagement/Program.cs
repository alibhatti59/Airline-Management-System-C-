using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirlineManagement
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AirlineManagement.Database.DatabaseInit.EnsureTablesExist();
            }
            catch
            {
                
            }

            // If presentation flag provided, open the fullscreen PresentationHost
            var args = Environment.GetCommandLineArgs();
            if (args != null && args.Any(a => a.Equals("--present", StringComparison.OrdinalIgnoreCase)))
            {
                PresentationHost.ShowHost();
            }
            else
            {
                // Start the presentation host so the app shows on the blank presentation background
                Application.Run(new PresentationHost());
            }
        }
    }
}
