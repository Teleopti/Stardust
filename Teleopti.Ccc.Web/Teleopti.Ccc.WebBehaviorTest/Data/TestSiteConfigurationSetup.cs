using System;
using System.IO;
using System.Threading;
using CassiniDev;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData.nhib.xml");

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
			var contents = File.ReadAllText("Data\\TestData.nhib.xml");
			contents = contents.Replace("_connectionString_", IniFileInfo.ConnectionString);
			contents = contents.Replace("_connectionStringMatrix_", IniFileInfo.ConnectionStringMatrix);
			contents = contents.Replace("_database_", IniFileInfo.Database);
			File.WriteAllText(TargetTestDataNHibFile, contents);
		}
	}
}