using System;
using System.IO;
using IISExpressAutomation;
using Teleopti.Ccc.Domain;
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

		private static IISExpress _server;
		private static IDisposable _portsConfiguration;

		//always use 64bit version? Don't want to change old behavior currently though...
		public const string PathToIISExpress32 = "c:\\program files (x86)\\IIS Express\\iisexpress.exe";
		public const string PathToIISExpress64 = "c:\\program files\\IIS Express\\iisexpress.exe";

		public static void Setup(string pathToIISExpress)
		{
			_portsConfiguration = RandomPortsAndUrls();
			writeWebConfigs();
			StartIISExpress(pathToIISExpress);
		}

		private static IDisposable RandomPortsAndUrls()
		{
			var originalPort = Port;
			var originalAuthenticationBridgePort = PortAuthenticationBridge;
			var originalWindowsIdentityProviderPort = PortWindowsIdentityProvider;

			Port = new Random().Next(57000, 57999);
			PortAuthenticationBridge = Port - new Random().Next(1, 100);
			PortWindowsIdentityProvider = Port + new Random().Next(1, 100);

			URL = new Uri(string.Format("http://localhost:{0}/", Port));
			UrlAuthenticationBridge = new Uri(string.Format("http://localhost:{0}/", PortAuthenticationBridge));
			WindowsClaimProvider = string.Format("<add identifier=\"urn:Windows\" displayName=\"Windows\" url=\"http://localhost:{0}/\" protocolHandler=\"OpenIdHandler\" />", PortWindowsIdentityProvider);
			TeleoptiClaimProvider = string.Format("<add identifier=\"urn:Teleopti\" displayName=\"Teleopti\" url=\"http://localhost:{0}/sso/\" protocolHandler=\"OpenIdHandler\" />", Port);

			return new GenericDisposable(() =>
			{
				Port = originalPort;
				URL = new Uri(string.Format("http://localhost:{0}/", originalPort));
				PortAuthenticationBridge = originalAuthenticationBridgePort;
				UrlAuthenticationBridge = new Uri(string.Format("http://localhost:{0}/", PortAuthenticationBridge));
				PortWindowsIdentityProvider = originalWindowsIdentityProviderPort;
				WindowsClaimProvider = string.Format("<add identifier=\"urn:Windows\" displayName=\"Windows\" url=\"http://localhost:{0}/\" protocolHandler=\"OpenIdHandler\" />", PortWindowsIdentityProvider);
				TeleoptiClaimProvider = string.Format("<add identifier=\"urn:Teleopti\" displayName=\"Teleopti\" url=\"http://localhost:{0}/sso/\" protocolHandler=\"OpenIdHandler\" />", originalPort);
			});
		}

		private static void StartIISExpress(string pathToIisExpress)
		{
			cleanOldLogs();
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			var runningConfig = "iisexpress.running.config";

			new FileConfigurator().Configure(
				"iisexpress.config",
				runningConfig,
				new TagsForTestSite().Make()
				);

			var parameters = new Parameters
			{
				Systray = false,
				Config = string.Concat(runningConfig, " /apppool:\"Clr4IntegratedAppPool\"")
			};
			_server = new IISExpress(parameters, pathToIisExpress);
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
				if (_server != null)
				{
					_server.Dispose();
				}
			}
			catch (NullReferenceException)
			{
				//sometimes throws "Process window not found" - don't make that exception bubble up in this teardown...
				//https://github.com/ElemarJR/IISExpress.Automation/blob/master/src/IISExpress.Automation/ProcessEnvelope.cs
			}
			if (_portsConfiguration != null)
			{
				_portsConfiguration.Dispose();
			}
			writeWebConfigs();
		}

		private static void writeWebConfigs()
		{
			writeWebConfig("web.root.web.config", Paths.WebPath());
			writeWebConfig("web.AuthenticationBridge.web.config", Paths.WebAuthenticationBridgePath());
			writeWebConfig("web.WindowsIdentityProvider.web.config", Paths.WebWindowsIdentityProviderPath());
		}
		
		private static void writeWebConfig(string sourceFileName, string targetFolder)
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), sourceFileName);
			var targetFile = Path.Combine(targetFolder, "web.config");

			new FileConfigurator().Configure(
				sourceFile,
				targetFile,
				new TagsForTestSite().Make()
				);
		}

		public static void StartApplicationAsync()
		{
			Http.GetAsync("");
		}

		public static void RecycleApplication()
		{
			var file = Path.Combine(Paths.WebBinPath(), "touch");
			File.WriteAllText(file, "can't touch this");
			File.Delete(file);
		}
	}

}