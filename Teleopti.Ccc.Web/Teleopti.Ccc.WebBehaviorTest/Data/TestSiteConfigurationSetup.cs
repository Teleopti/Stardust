using System;
using System.IO;
using System.Linq;
using System.Threading;
using CassiniDev;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestSiteConfigurationSetup));

		private static readonly string AgentPortalWebNhibConfPath = Path.Combine(IniFileInfo.SitePath, "bin");
		private static readonly string TargetTestDataNHibFile = Path.Combine(AgentPortalWebNhibConfPath, "TestData.nhib.xml");

		public static Uri Url;
		
		private static Server _server;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void Setup()
		{
			var startTime = DateTime.Now;

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

			var setupTime = DateTime.Now.Subtract(startTime);
			Log.Write("Test site setup took " + setupTime);
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

	}
}