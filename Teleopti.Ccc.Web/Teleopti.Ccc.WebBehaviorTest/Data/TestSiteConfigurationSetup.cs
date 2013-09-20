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

		private static IISExpress _server;

		public static void Setup()
		{
			Url = new Uri(IniFileInfo.Url);

			// attempt IIS express
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			if (IniFileInfo.IISExpress)
			{
				try
				{
					var port = new Random().Next(57000, 57999);
					_server = new IISExpress(new Parameters
					{
						Path = Paths.WebPath(),
						Port = port,
						Systray = true
					});
					Url = new Uri(string.Format("http://localhost:{0}/", port));
				}
				catch (Exception)
				{
				}
			} 

			UpdateWebConfigFromTemplate();
			GenerateAndWriteTestDataNHibFileFromTemplate();
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