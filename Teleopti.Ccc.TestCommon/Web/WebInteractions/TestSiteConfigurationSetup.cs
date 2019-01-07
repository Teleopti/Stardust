using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IISExpressAutomation;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Support.Library.Config;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public static class TestSiteConfigurationSetup
	{
		public static int Port = 52858;
		public static Uri URL;
		public static int PortAuthenticationBridge = 52857;
		public static Uri UrlAuthenticationBridge;
		public static int PortWindowsIdentityProvider = 52859;
		public static string WindowsClaimProvider;
		public static string TeleoptiClaimProvider;
		public static string UrlWindowsIdentityProvider;
		private static readonly ILog _logger = LogManager.GetLogger(typeof(TestSiteConfigurationSetup));

		private static IisExpressRunner _server;
		private static Process _stardustProcess;
		private static IDisposable _portsConfiguration;

		private static readonly SettingsFileManager settingsFile = new SettingsFileManager();

		public static void Setup(bool runStardust = false, TestLog testlog = null)
		{
			_portsConfiguration = RandomPortsAndUrls(testlog);
			
			writeWebConfigs();
			killProcess("killAllIISExpress", "iisexpress");
			StartIISExpress();
			
			if (runStardust)
			{
				StartStardust();
			}
		}

		public static void StartStardust()
		{
			KillStardust();
			
			WriteConfig("ServiceBusHost.config", Paths.StardustConsoleHostFolderPath(), "Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe.config");
			
			var starDustConsoleHostPath = $"{Paths.StardustConsoleHostFolderPath()}\\Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe";

			if (!File.Exists(starDustConsoleHostPath))
				throw new ArgumentException("Stardust console host executable not found", starDustConsoleHostPath);
			_stardustProcess = Process.Start(new ProcessStartInfo
			{
				FileName = starDustConsoleHostPath
			});
		}

		private static int customPortLastUsed = 57000;

		private static IDisposable RandomPortsAndUrls(TestLog testlog = null)
		{
			var originalPort = Port;
			var originalAuthenticationBridgePort = PortAuthenticationBridge;
			var originalWindowsIdentityProviderPort = PortWindowsIdentityProvider;

			Port = customPortLastUsed++; //new Random().Next(57000, 57999);
			PortAuthenticationBridge = customPortLastUsed++; //Port - new Random().Next(1, 100);
			PortWindowsIdentityProvider = customPortLastUsed++; // Port + new Random().Next(1, 100);

			testlog?.Debug($"Port={Port} ; PortAuthenticationBridge={PortAuthenticationBridge} ; PortWindowsIdentityProvider={PortWindowsIdentityProvider}");
			
			URL = new Uri($"http://localhost:{Port}/");
			UrlAuthenticationBridge = new Uri($"http://localhost:{PortAuthenticationBridge}/");
			WindowsClaimProvider =
				$"<add identifier=\"urn:Windows\" displayName=\"Windows\" url=\"http://localhost:{PortWindowsIdentityProvider}/\" protocolHandler=\"OpenIdHandler\" />";
			UrlWindowsIdentityProvider = $"http://localhost:{PortWindowsIdentityProvider}/";
			TeleoptiClaimProvider =
				$"<add identifier=\"urn:Teleopti\" displayName=\"Teleopti\" url=\"http://localhost:{Port}/sso/\" protocolHandler=\"OpenIdHandler\" />";

			return new GenericDisposable(() =>
			{
				Port = originalPort;
				URL = new Uri($"http://localhost:{originalPort}/");
				PortAuthenticationBridge = originalAuthenticationBridgePort;
				UrlAuthenticationBridge = new Uri($"http://localhost:{PortAuthenticationBridge}/");
				PortWindowsIdentityProvider = originalWindowsIdentityProviderPort;
				WindowsClaimProvider =
					$"<add identifier=\"urn:Windows\" displayName=\"Windows\" url=\"http://localhost:{PortWindowsIdentityProvider}/\" protocolHandler=\"OpenIdHandler\" />";
				UrlWindowsIdentityProvider = $"http://localhost:{PortWindowsIdentityProvider}/";
				TeleoptiClaimProvider =
					$"<add identifier=\"urn:Teleopti\" displayName=\"Teleopti\" url=\"http://localhost:{originalPort}/sso/\" protocolHandler=\"OpenIdHandler\" />";
			});
		}


		private static void StartIISExpress()
		{
			cleanOldLogs();
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			var runningConfig = "iisexpress.running.config";

			new FileConfigurator().Configure(
				"iisexpress.config",
				runningConfig,
				searchReplaces()
			);

			//Process.Start()

			var parameters = new Parameters
			{
				Systray = false,
				Config = string.Concat(runningConfig, " /apppool:\"Clr4IntegratedAppPool\"")
			};
			//_server = new IISExpress(parameters, "c:\\program files\\IIS Express\\iisexpress.exe");
			_server = new IisExpressRunner();
			_server.Execute(parameters, "c:\\program files\\IIS Express\\iisexpress.exe");
		}

		private static void cleanOldLogs()
		{
			//this folder is hard coded in iisexpress.config
			const string logFolder = @"c:\temp\iisexpresslogs";
			if (Directory.Exists(logFolder))
			{
				try
				{
					Directory.Delete(logFolder, true);
				}
				catch (IOException)
				{
					//Ignore if files are locked - The lock sometimes is still there -> timing issue
					//doesn't really matter. Will be removed next run
				}
			}
		}

		public static void TearDown()
		{
			try
			{
				_stardustProcess?.Dispose();
				_server?.Dispose();
			}
			catch (InvalidOperationException ex)
			{
				_logger.Error(ex.InnerException);
			}
			catch (NullReferenceException ex)
			{
				_logger.Error(ex.InnerException);
				//sometimes throws "Process window not found" - don't make that exception bubble up in this teardown...
				//https://github.com/ElemarJR/IISExpress.Automation/blob/master/src/IISExpress.Automation/ProcessEnvelope.cs
			}

			killProcess("killAllIISExpress", "iisexpress");
			waitToBeKilled("waittobekilled", "iisexpress");
			KillStardust();

			_portsConfiguration?.Dispose();
			writeWebConfigs();
		}

		public static void KillStardust()
		{
			killProcess("killStardustConsoleHost", "Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost");
			waitToBeKilled("waitStardustConsoleHostToBeKilled", "Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost");
		}

		private static void waitToBeKilled(string logName, string processName)
		{
			TestLog.Static.Debug(logName);
			Process.GetProcessesByName(processName)
				.ForEach(p =>
				{
					TestLog.Static.Debug($"{logName}/wait");
					if (!p.WaitForExit(2000))
					{
						TestLog.Static.Debug($"Couldn\'t kill process {processName}!!");
					}
				});
			TestLog.Static.Debug($"/{logName}");
		}

		private static void killProcess(string logName, string processName)
		{
			TestLog.Static.Debug(logName);
			Process.GetProcessesByName(processName)
				.ForEach(p =>
				{
					if (!p.HasExited)
					{
						TestLog.Static.Debug($"{logName}/kill");
						p.Kill();
					}
				});
			TestLog.Static.Debug($"/{logName}");
		}

		private static void writeWebConfigs()
		{
			var targetFolder = Paths.WebPath();
			var appInsightsConfig = Path.Combine(targetFolder,"ApplicationInsights.config");
			if (File.Exists(appInsightsConfig))
			{
				File.Delete(appInsightsConfig);
			}
			writeWebConfig("web.root.web.config", targetFolder);
			writeWebConfig("web.AuthenticationBridge.web.config", Paths.WebAuthenticationBridgePath());
			writeWebConfig("web.WindowsIdentityProvider.web.config", Paths.WebWindowsIdentityProviderPath());
		}

		private static void writeWebConfig(string sourceFileName, string targetFolder)
		{
			WriteConfig(sourceFileName, targetFolder, "web.config");
		}

		private static void WriteConfig(string sourceFileName, string targetFolder, string targetFileName)
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), sourceFileName);
			var targetFile = Path.Combine(targetFolder, targetFileName);

			new FileConfigurator().Configure(
				sourceFile,
				targetFile,
				searchReplaces()
			);
		}

		public static void StartApplicationAsync()
		{
			using (var h = new Http())
				h.GetAsync("Test/Start");
		}

		public static void StartApplicationSync()
		{
			using (var h = new Http())
				h.Get("Test/Start");
		}

		public static void RecycleApplication()
		{
			var file = Path.Combine(Paths.WebPath(), "app_offline.htm");
			File.WriteAllText(file, "<html><body>Offline!</body></html>");
			Thread.Sleep(TimeSpan.FromSeconds(5));
			File.Delete(file);
			Thread.Sleep(TimeSpan.FromSeconds(5));
		}

		private static SearchReplaceCollection searchReplaces()
		{
			// infra test config
			var file = Path.Combine(Paths.FindProjectPath(@".debug-Setup\config\"), "settings.txt");
			settingsFile.LoadFile(file);
			settingsFile.UpdateFileByName("machineKey.validationKey", "754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1");
			settingsFile.UpdateFileByName("machineKey.decryptionKey", "3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429");
			settingsFile.UpdateFileByName("DB_CCC7", InfraTestConfigReader.DatabaseName);
			settingsFile.UpdateFileByName("DB_ANALYTICS", InfraTestConfigReader.AnalyticsDatabaseName);
			settingsFile.UpdateFileByName("AS_DATABASE", InfraTestConfigReader.AnalyticsDatabaseName);
			settingsFile.UpdateFileByName("SQL_AUTH_STRING", InfraTestConfigReader.SqlAuthString);
			settingsFile.UpdateFileByName("ToggleMode", InfraTestConfigReader.ToggleMode);

			// behavior test
			settingsFile.UpdateFileByName("TestLogConfiguration", "<logger name='Teleopti.TestLog'><level value='DEBUG'/></logger><logger name='NHibernate'><level value='WARN'/></logger>");
			settingsFile.UpdateFileByName("CheckNewJobIntervalSeconds", "<add key='CheckNewJobIntervalSeconds' value='5'/>");
			settingsFile.UpdateFileByName("BehaviorTestServer", "true");
			settingsFile.UpdateFileByName("MessagesOnBoot", "false");
			settingsFile.UpdateFileByName("HangfireDashboard", "true");
			settingsFile.UpdateFileByName("HangfireDashboardStatistics", "true");
			settingsFile.UpdateFileByName("HangfireDashboardCounters", "true");
			settingsFile.UpdateFileByName("HangfireJobExpirationSeconds", TimeSpan.FromDays(1).TotalSeconds.ToString());
			settingsFile.UpdateFile("<customErrors mode=\"On\" defaultRedirect=\"~/content/error/error.htm\">", "<customErrors mode=\"Off\" defaultRedirect=\"~/content/error/error.htm\">");

			// iisexpress
			settingsFile.UpdateFileByName("Port", Port.ToString());
			settingsFile.UpdateFileByName("PortAuthenticationBridge", PortAuthenticationBridge.ToString());
			settingsFile.UpdateFileByName("PortWindowsIdentityProvider", PortWindowsIdentityProvider.ToString());
			settingsFile.UpdateFileByName("SitePath", Paths.WebPath());
			settingsFile.UpdateFileByName("SitePathAuthenticationBridge", Paths.WebAuthenticationBridgePath());
			settingsFile.UpdateFileByName("SitePathWindowsIdentityProvider", Paths.WebWindowsIdentityProviderPath());

			// urls?
			settingsFile.UpdateFileByName("URL", URL.ToString());
			settingsFile.UpdateFileByName("UrlAuthenticationBridge", UrlAuthenticationBridge.ToString());
			settingsFile.UpdateFileByName("WindowsClaimProvider", WindowsClaimProvider);
			settingsFile.UpdateFileByName("TeleoptiClaimProvider", TeleoptiClaimProvider);
			settingsFile.UpdateFileByName("URL_WINDOWS_IDENTITY_PROVIDER", UrlWindowsIdentityProvider);

			return settingsFile.ReadFile();
		}
	}
}