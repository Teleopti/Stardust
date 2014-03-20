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
		private static readonly string TargetAuthenticationBridgeWebConfig = Path.Combine(Paths.FindProjectPath(@"Teleopti.Ccc.Web.AuthenticationBridge\"), "web.config");
		private static readonly string BackupAuthenticationBridgeWebConfig = Path.Combine(Paths.FindProjectPath(@"Teleopti.Ccc.Web.AuthenticationBridge\"), "web.backup.config");
		private static readonly string TargetWindowsIdentityProviderWebConfig = Path.Combine(Paths.FindProjectPath(@"Teleopti.Ccc.Web.WindowsIdentityProvider\"), "web.config");
		private static readonly string BackupWindowsIdentityProviderWebConfig = Path.Combine(Paths.FindProjectPath(@"Teleopti.Ccc.Web.WindowsIdentityProvider\"), "web.backup.config");

		public static Uri Url;
		public static int Port;

		public static Uri UrlAuthenticationBridge;
		public static int PortAuthenticationBridge;

		public static Uri UrlWindowsIdentityProvider;
		public static int PortWindowsIdentityProvider;



		private static IISExpress _server;

		public static void Setup()
		{
			Url = new Uri(IniFileInfo.Url);
			Port = Url.Port;

			if (IniFileInfo.IISExpress)
			{
				getPortAndUrl();
				UpdateWebConfigFromTemplate();
				UpdateAuthenticationBridgeWebConfigFromTemplate();
				UpdateWindowsIdentityProviderWebConfigFromTemplate();
				AttemptToUseIISExpress();
			}
			else
			{
				UpdateWebConfigFromTemplate();
				UpdateAuthenticationBridgeWebConfigFromTemplate();
				UpdateWindowsIdentityProviderWebConfigFromTemplate();
			}
			
			GenerateAndWriteTestDataNHibFileFromTemplate();
		}

		private static void AttemptToUseIISExpress()
		{
			// attempt IIS express
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			try
			{
				FileConfigurator.ConfigureByTags("Data\\iisexpress.config", "Data\\iisexpress.running.config", new AllTags());
				var parameters = new Parameters
					{
						Systray = true,
						Config = "Data\\iisexpress.running.config /apppool:\"Clr4IntegratedAppPool\""
					};
				_server = new IISExpress(parameters);
			}
			catch (Exception)
			{
				Url = new Uri(IniFileInfo.Url);
				Port = Url.Port;
			}
		}

		private static void getPortAndUrl()
		{
			Port = new Random().Next(57000, 57999);
			Url = new Uri(string.Format("http://localhost:{0}/", Port));

			PortAuthenticationBridge = Port - 1;
			UrlAuthenticationBridge = new Uri(string.Format("http://localhost:{0}/", PortAuthenticationBridge));

			PortWindowsIdentityProvider = Port + 1;
			UrlWindowsIdentityProvider = new Uri(string.Format("http://localhost:{0}/", PortWindowsIdentityProvider));
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

			if (File.Exists(TargetWebConfig))
				File.Copy(TargetWebConfig, BackupWebConfig, true);

			FileConfigurator.ConfigureByTags(
				sourceFile,
				TargetWebConfig,
				tags
				);

			File.Copy(TargetWebConfig, BehaviorTestWebConfig, true);
		}

		private static void UpdateAuthenticationBridgeWebConfigFromTemplate()
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), "web.AuthenticationBridge.web.config");

			var tags = new AllTags();

			if (File.Exists(TargetAuthenticationBridgeWebConfig))
				File.Copy(TargetAuthenticationBridgeWebConfig, BackupAuthenticationBridgeWebConfig, true);

			FileConfigurator.ConfigureByTags(
				sourceFile,
				TargetAuthenticationBridgeWebConfig,
				tags
				);
		}

		private static void UpdateWindowsIdentityProviderWebConfigFromTemplate()
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), "web.WindowsIdentityProvider.web.config");

			var tags = new AllTags();

			if (File.Exists(TargetWindowsIdentityProviderWebConfig))
				File.Copy(TargetWindowsIdentityProviderWebConfig, BackupWindowsIdentityProviderWebConfig, true);

			FileConfigurator.ConfigureByTags(
				sourceFile,
				TargetWindowsIdentityProviderWebConfig,
				tags
				);
		}


		private static void RevertWebConfig()
		{
			if (File.Exists(BackupWebConfig))
				File.Copy(BackupWebConfig, TargetWebConfig, true);
			if (File.Exists(BackupAuthenticationBridgeWebConfig))
				File.Copy(BackupAuthenticationBridgeWebConfig, TargetAuthenticationBridgeWebConfig, true);
			if (File.Exists(BackupAuthenticationBridgeWebConfig))
				File.Copy(BackupAuthenticationBridgeWebConfig, TargetAuthenticationBridgeWebConfig, true);
		}
	}
}