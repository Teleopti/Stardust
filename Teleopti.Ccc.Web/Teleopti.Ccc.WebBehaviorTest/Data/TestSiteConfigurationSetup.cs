﻿using System;
using System.IO;
using IISExpressAutomation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestSiteConfigurationSetup
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData.nhib.xml");

		public static int Port = 52858;
		public static Uri URL;
		public static int PortAuthenticationBridge = 52857;
		public static Uri UrlAuthenticationBridge;
		public static int PortWindowsIdentityProvider = 52859;
		public static string WindowsClaimProvider;
		public static string TeleoptiClaimProvider;

		private static IISExpress _server;
		private static IDisposable _portsConfiguration;

		public static void Setup()
		{
			_portsConfiguration = RandomPortsAndUrls();
			WriteWebConfigs();
			StartIISExpress();
			GenerateAndWriteTestDataNHibFileFromTemplate();
		}

		private static IDisposable RandomPortsAndUrls()
		{
			var originalPort = Port;
			var originalAuthenticationBridgePort = PortAuthenticationBridge;
			var originalWindowsIdentityProviderPort = PortWindowsIdentityProvider;

			Port = new Random().Next(57000, 57999);
			URL = new Uri(string.Format("http://localhost:{0}/", Port));
			PortAuthenticationBridge = Port - new Random().Next(1, 100);
			UrlAuthenticationBridge = new Uri(string.Format("http://localhost:{0}/", PortAuthenticationBridge));
			PortWindowsIdentityProvider = Port + new Random().Next(1, 100);
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
			FileConfigurator.ConfigureByTags("Data\\iisexpress.config", "Data\\iisexpress.running.config", new AllTags());
			var parameters = new Parameters
				{
					Systray = true,
					Config = "Data\\iisexpress.running.config /apppool:\"Clr4IntegratedAppPool\""
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

			var tags = new AllTags();
			const string module = @"<module type=""Teleopti.Ccc.IocCommon.Configuration.SyncEventsPublisherModule, Teleopti.Ccc.IocCommon""/>";
				tags.Add(
					"SyncEventsPublisherModule",
					module
					);

			FileConfigurator.ConfigureByTags(
				sourceFile,
				targetFile,
				tags
				);
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

	}
}