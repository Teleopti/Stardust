using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Autofac;
using log4net.Config;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Gui;
using Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Application = System.Windows.Application;

namespace Teleopti.Analytics.Etl.ConfigTool
{
	public partial class App : Application
	{
		public static IContainer Container;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			XmlConfigurator.Configure();
			var builder = new ContainerBuilder();
			builder.RegisterModule(new EtlAppModule());
			Container = builder.Build();

			var webEtlRedirectView = new WebEtlRedirectView();
			if (webEtlRedirectView.ShowDialog() == DialogResult.Yes)
			{
				var wfmPath = Container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
				var path = wfmPath.Replace("Web", "Administration");
				System.Diagnostics.Process.Start(path);
				return;
			}
			webEtlRedirectView.Close();

			var configurationHandler = new ConfigurationHandler(new GeneralFunctions(new GeneralInfrastructure(new BaseConfigurationRepository())), new BaseConfigurationValidator());
			configurationHandler.SetConnectionString(ConfigurationManager.AppSettings["datamartConnectionString"]);

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
				typeof (FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

			var startWindow = new MainWindow(configurationHandler.BaseConfiguration);
			startWindow.Show();
		}
	}
}