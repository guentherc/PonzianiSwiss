using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Mode mode = Mode.Release;
            int indx = 0;
            for (indx = 0; indx < e.Args.Length - 1; ++indx)
            {
                if (e.Args[indx] == "-mode")
                {
                    if (Enum.TryParse<Mode>(e.Args[indx + 1], out Mode m))
                       mode = m;
                }
            }
            MainWindow wnd = new(mode);
            wnd.Show();
        }

        public enum Mode { Release, Test }
    }
}
