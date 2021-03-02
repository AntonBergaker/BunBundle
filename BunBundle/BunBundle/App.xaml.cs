using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace BunBundle {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            Timeline.SpeedRatioProperty.OverrideMetadata(typeof(Storyboard), new FrameworkPropertyMetadata { DefaultValue = 2d });
            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }
    }
}
