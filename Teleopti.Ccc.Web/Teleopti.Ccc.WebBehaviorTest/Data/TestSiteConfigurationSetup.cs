using System;
using System.IO;
using System.Linq;
using CassiniDev;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestSiteConfigurationSetup));

		private static readonly string AgentPortalWebNhibConfPath = Path.Combine(IniFileInfo.SitePath, "bin");
		private static readonly string TargetTestDataNHibFile = Path.Combine(AgentPortalWebNhibConfPath, "TestData.nhib.xml");
		private static readonly string TestDataAutoFacOverrideonfigurationFile = Path.Combine(AgentPortalWebNhibConfPath, "TestData.autofac.config");


		public static Uri Url;
		
		private static Server _server;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void Setup()
		{
			var startTime = DateTime.Now;
			
			addTestDataAutoFacOverrideFile();

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
			//BackupExistingNHibFiles();
			GenerateAndWriteTestDataNHibFileFromTemplate();
			

			var setupTime = DateTime.Now.Subtract(startTime);
			Log.Write("Test site setup took " + setupTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void TearDown()
		{
			if (IniFileInfo.CassiniDev)
				_server.ShutDown();

			//RemoveTestDataNHibFile();
			//RevertBackedUpNHibFiles();

			removeTestDataAutoFacOverrideFile();
		}

		private static void RemoveTestDataNHibFile()
		{
			File.SetAttributes(TargetTestDataNHibFile, FileAttributes.Archive);
			File.Delete(TargetTestDataNHibFile);
		}

		private static void GenerateAndWriteTestDataNHibFileFromTemplate()
		{
			var contents = File.ReadAllText("Data\\TestData.nhib.xml");
			contents = contents.Replace("_connectionString_", IniFileInfo.ConnectionString);
			contents = contents.Replace("_connectionStringMatrix_", IniFileInfo.ConnectionStringMatrix);
			contents = contents.Replace("_database_", IniFileInfo.Database);
			File.WriteAllText(TargetTestDataNHibFile, contents);
		}

		private static void BackupExistingNHibFiles()
		{
			var existingNHibFiles = Directory.GetFiles(AgentPortalWebNhibConfPath, "*.nhib.xml");
			existingNHibFiles.ToList().ForEach(f =>
			                                   	{
			                                   		var newFile = f + ".bak";
			                                   		File.SetAttributes(f, FileAttributes.Archive);
			                                   		if (File.Exists(newFile))
			                                   		{
			                                   			File.Delete(newFile);
			                                   			//File.SetAttributes(newFile, FileAttributes.Archive);
			                                   		}
			                                   		File.Move(f, newFile);
			                                   	});
		}

		private static void RevertBackedUpNHibFiles()
		{
			var backedUpNHibFiles = Directory.GetFiles(AgentPortalWebNhibConfPath, "*.nhib.xml.bak");
			backedUpNHibFiles.ToList().ForEach(f =>
			                                   	{
			                                   		var newFile = f.Substring(0, f.Length - 4);
			                                   		File.SetAttributes(f, FileAttributes.Archive);
			                                   		File.Move(f, newFile);
			                                   	});
		}

		private static void addTestDataAutoFacOverrideFile()
		{
			File.Copy("Data\\TestData.autofac.config", TestDataAutoFacOverrideonfigurationFile, true);
		}

		private static void removeTestDataAutoFacOverrideFile()
		{
			File.Delete(TestDataAutoFacOverrideonfigurationFile);
		}
	}
}