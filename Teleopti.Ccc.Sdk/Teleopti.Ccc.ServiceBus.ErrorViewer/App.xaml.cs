using System.Windows;
using Rhino.ServiceBus.Hosting;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DefaultHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _host = new DefaultHost();
            _host.Start<SimpleBootStrapper>();
            Global.SetContainer(_host.Container);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_host!=null)
            {
                _host.Dispose();
                _host = null;
            }
        }
    }
}
