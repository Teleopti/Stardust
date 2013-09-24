using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using CassiniDev;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData.nhib.xml");
		private static readonly string TargetWebConfig = Path.Combine(Paths.WebPath(), "web.config");

		public static Uri Url;
		
		private static Server _server;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void Setup()
		{
			if (IniFileInfo.CassiniDev)
			{
				Url = new Uri("http://localhost:57567/");
				_server = new Server(57567, IniFileInfo.AGENTPORTALWEB_nhibConfPath);
				_server.Start();
			} 
			else
			{
				Url = new Uri(IniFileInfo.Url);
			}
			UpdateWebConfigFromTemplate();
			GenerateAndWriteTestDataNHibFileFromTemplate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void TearDown()
		{
			if (IniFileInfo.CassiniDev)
				_server.ShutDown();
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