using System;
using System.IO;
using IISExpressAutomation;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb
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

		private const bool useIisExpressStartedByVisualStudio = false;

		public static void Setup()
		{
			_portsConfiguration = RandomPortsAndUrls();
			WriteWebConfigs();
			if (!useIisExpressStartedByVisualStudio)
				StartIISExpress();
		}

		private static IDisposable RandomPortsAndUrls()
		{
			var originalPort = Port;
			var originalAuthenticationBridgePort = PortAuthenticationBridge;
			var originalWindowsIdentityProviderPort = PortWindowsIdentityProvider;

			if (!useIisExpressStartedByVisualStudio)
			{
				Port = new Random().Next(57000, 57999);
				PortAuthenticationBridge = Port - new Random().Next(1, 100);
				PortWindowsIdentityProvider = Port + new Random().Next(1, 100);
			}
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

		private static void StartIISExpress()
		{
			// maybe this SO thread contains alternatives:
			// http://stackoverflow.com/questions/4772092/starting-and-stopping-iis-express-programmatically
			var runningConfig = "iisexpress.running.config";
			FileConfigurator.ConfigureByTags("iisexpress.config", runningConfig, new AllTags());
			var parameters = new Parameters
				{
					Systray = false,
					Config = string.Concat(runningConfig,  " /apppool:\"Clr4IntegratedAppPool\"")
				};
			_server = new IISExpress(parameters);
		}

		public static void TearDown()
		{
			_server.Dispose();
			_portsConfiguration.Dispose();
			WriteWebConfigs();
		}

		private static void WriteWebConfigs()
		{
			WriteWebConfig(new ConfigOptions
			{
				SourceFileName = "web.root.web.config", 
				TargetFolder = Paths.WebPath()
			});
			WriteWebConfig(new ConfigOptions
			{
				SourceFileName = "web.AuthenticationBridge.web.config",
				TargetFolder = Paths.WebAuthenticationBridgePath()
			});
			WriteWebConfig(new ConfigOptions
			{
				SourceFileName = "web.WindowsIdentityProvider.web.config",
				TargetFolder = Paths.WebWindowsIdentityProviderPath()
			});
		}

		private class ConfigOptions
		{
			public string SourceFileName { get; set; }
			public string TargetFolder { get; set; }
		}

		private static void WriteWebConfig(ConfigOptions o)
		{
			var sourceFile = Path.Combine(Paths.FindProjectPath(@"BuildArtifacts\"), o.SourceFileName);
			var targetFile = Path.Combine(o.TargetFolder, "web.config");

			var tags = new AllTags {{"PublishEventsToServiceBus", "false"}};

			FileConfigurator.ConfigureByTags(
				sourceFile,
				targetFile,
				tags
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