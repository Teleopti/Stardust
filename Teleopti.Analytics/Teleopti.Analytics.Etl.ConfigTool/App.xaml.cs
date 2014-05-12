using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration;
using Teleopti.Analytics.Etl.Transformer;
using log4net.Config;
using System.Xaml;
using Teleopti.Interfaces.Domain;
using Application = System.Windows.Application;

namespace Teleopti.Analytics.Etl.ConfigTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			XmlConfigurator.Configure();

			var configurationHandler = new ConfigurationHandler(new GeneralFunctions(ConfigurationManager.AppSettings["datamartConnectionString"]));

			if (!configurationHandler.IsConfigurationValid)
			{
				var startupConfigurationView = new StartupConfigurationView(configurationHandler);
				if (startupConfigurationView.ShowDialog() == DialogResult.Cancel)
				{
					Current.Shutdown();
					return;
				}
			}

			if (configurationHandler.BaseConfiguration.CultureId != null)
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(configurationHandler.BaseConfiguration.CultureId.Value).FixPersianCulture();

			// WPF should use CurrentCulture
			FrameworkElement.LanguageProperty.OverrideMetadata(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

			var startWindow = new MainWindow(configurationHandler.BaseConfiguration);
			startWindow.Show();
		}
	}
}