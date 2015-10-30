using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Autofac;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration;
using log4net.Config;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
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
			IContainer container = configureContainer();

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
			
			var startWindow = new MainWindow(configurationHandler.BaseConfiguration,container);
			startWindow.Show();
		}

		 private static IContainer configureContainer()
		 {
			 var builder = new ContainerBuilder();
			 var iocArgs = new IocArgs(new ConfigReader());
			 var configuration = new IocConfiguration(
							iocArgs,
							CommonModule.ToggleManagerForIoc(iocArgs));

			 builder.RegisterModule(new CommonModule(configuration));
			 builder.RegisterModule(new EtlModule(configuration));
			 
			 return builder.Build();

		 }

	 }
}