using System;
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
				_server = new Server(57567, IniFileInfo.SitePath);
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

		public static void RestartApplication()
		{
			// just to make sure we'r not on the same second.
			// Not even sure this is required to make the touch valid at all times
			Thread.Sleep(1010);
			// touch the nhib file in the bin folder to make the app restart
			File.SetLastWriteTimeUtc(TargetTestDataNHibFile, DateTime.UtcNow);
		}

		private static void GenerateAndWriteTestDataNHibFileFromTemplate()
		{
			var contents = File.ReadAllText("Data\\TestData.nhib.xml");
			contents = contents.Replace("_connectionString_", IniFileInfo.ConnectionString);
			contents = contents.Replace("_connectionStringMatrix_", IniFileInfo.ConnectionStringMatrix);
			contents = contents.Replace("_database_", IniFileInfo.Database);
			File.WriteAllText(TargetTestDataNHibFile, contents);
		}
		
		private static void UpdateWebConfigFromTemplate()
		{
			var contents = File.ReadAllText(Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), "web.root.web.config"));
			contents = contents.Replace("$(AgentPortalWebURL)", Url.ToString());
			contents = contents.Replace("$(AnalyticsDB)", new SqlConnectionStringBuilder(IniFileInfo.ConnectionStringMatrix).InitialCatalog);
			File.WriteAllText(TargetWebConfig, contents);
		}
	}
}