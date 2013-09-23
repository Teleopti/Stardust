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

		public static Uri Url;
		public static int Port;

		private static IISExpress _server;

		public static void Setup()
		{
			Url = new Uri(IniFileInfo.Url);
			Port = Url.Port;

			if (IniFileInfo.IISExpress)
				AttemptToUseIISExpress();

			UpdateWebConfigFromTemplate();
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
		
		private static void UpdateWebConfigFromTemplate()
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), "web.root.web.config");
			var tags = new AllTags();
			if (!IniFileInfo.ServiceBus)
			{
				const string module = @"<module type=""Teleopti.Ccc.IocCommon.Configuration.LocalInMemoryEventsPublisherModule, Teleopti.Ccc.IocCommon""/>";
				tags.Add(
					"LocalInMemoryEventsPublisherModule",
					module
					);
			}
			FileConfigurator.ConfigureByTags(
				sourceFile,
				TargetWebConfig,
				tags
				);
		}
	}
}