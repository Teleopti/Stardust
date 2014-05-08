using System;
using System.IO;
using System.Threading;
using IISExpressAutomation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData.nhib.xml");
		private static readonly string TargetWebConfig = Path.Combine(Paths.WebPath(), "web.config");
		private static readonly string BackupWebConfig = Path.Combine(Paths.WebPath(), "web.backup.config");
		private static readonly string BehaviorTestWebConfig = Path.Combine(Paths.WebPath(), "web.fromtest.config");

		public static Uri Url;
		public static int Port;

		private static IISExpress _server;

		public static void Setup()
		{
			Url = new Uri(IniFileInfo.Url);
			Port = Url.Port;

			if (IniFileInfo.IISExpress)
				AttemptToUseIISExpress();

			updateWebConfigFromTemplate();
			GenerateAndWriteTestDataNHibFileFromTemplate();
		}

		private static void AttemptToUseIISExpress()
		{
			// attempt IIS express
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			try
			{
				Port = new Random().Next(57000, 57999);
				Url = new Uri(string.Format("http://localhost:{0}/", Port));
				FileConfigurator.ConfigureByTags("Data\\iisexpress.config", "Data\\iisexpress.running.config", new AllTags());
				_server = new IISExpress(new Parameters
					{
						Systray = true,
						Config = "Data\\iisexpress.running.config"
					});
			}
			catch (Exception)
			{
				Url = new Uri(IniFileInfo.Url);
				Port = Url.Port;
			}
		}

		public static void TearDown()
		{
			if (_server != null)
				_server.Dispose();

			RevertWebConfig();
		}

		public static void RecycleApplication()
		{
			var file = Path.Combine(Paths.WebBinPath(), "touch");
			File.WriteAllText(file, "can't touch this");
			File.Delete(file);
		}

		private static void GenerateAndWriteTestDataNHibFileFromTemplate()
		{
			FileConfigurator.ConfigureByTags(
				"Data\\TestData.nhib.xml",
				TargetTestDataNHibFile,
				new AllTags()
				);
		}
		
		private static void updateWebConfigFromTemplate()
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), "web.root.web.config");
			var tags = new AllTags();
			if (!IniFileInfo.ServiceBus)
			{
				const string module =
					@"<module type=""Teleopti.Ccc.IocCommon.Configuration.LocalInMemoryEventsPublisherModule, Teleopti.Ccc.IocCommon""/>";
				tags.Add(
					"LocalInMemoryEventsPublisherModule",
					module
					);
			}

			if (File.Exists(TargetWebConfig))
				File.Copy(TargetWebConfig, BackupWebConfig, true);

			FileConfigurator.ConfigureByTags(
				sourceFile,
				TargetWebConfig,
				tags
				);

			File.Copy(TargetWebConfig, BehaviorTestWebConfig, true);
		}

		private static void RevertWebConfig()
		{
			if (File.Exists(BackupWebConfig))
				File.Copy(BackupWebConfig, TargetWebConfig, true);
		}		
	}
}