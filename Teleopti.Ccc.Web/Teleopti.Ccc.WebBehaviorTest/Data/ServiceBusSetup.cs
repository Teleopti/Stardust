using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class ServiceBusSetup
	{
		private static Process _process;
		
		private static string ConsoleHostBuildPath()
		{
			return Paths.FindProjectPath(
				@"Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost\bin\Debug\",
				@"Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost\bin\Release\"
				);
		}

		private static string BuildArtifactsPath()
		{
			return Paths.FindProjectPath(@"BuildArtifacts\");
		}

		private static string ConsoleHostExecutablePath()
		{
			return Path.Combine(ConsoleHostBuildPath(), "Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe");
		}

		private static string ConsoleHostConfigTargetPath()
		{
			return Path.Combine(ConsoleHostBuildPath(), "Teleopti.Ccc.Sdk.ServiceBus.ConsoleHost.exe.config");
		}

		private static string ConsoleHostConfigSourcePath()
		{
			return Path.Combine(BuildArtifactsPath(), "ServiceBusHost.config");
		}



		public static void Setup()
		{
			Configure();
			StartProcess();
		}

		public static void TearDown()
		{
			CloseProcess();
		}



		private static void Configure()
		{
			var contents = File.ReadAllText(ConsoleHostConfigSourcePath());
			contents = ReplaceTag(contents, "ConfigPath", Paths.WebBinPath());
			contents = ReplaceTag(contents, "MessageBroker", TestSiteConfigurationSetup.Url.ToString());
			contents = ReplaceTag(contents, "MessagesOnBoot", "false");
			contents = ReplaceTag(contents, "AnalyticsConnectionString", IniFileInfo.ConnectionStringMatrix);
			File.WriteAllText(ConsoleHostConfigTargetPath(), contents);
		}

		private static string ReplaceTag(string contents, string tag, string value)
		{
			var appSettingTag = string.Format("<!--$({0}AppSetting)-->", tag);
			if (contents.Contains(appSettingTag))
			{
				var appSetting = string.Format(@"<add key=""{0}"" value=""{1}"" />", tag, value);
				return contents.Replace(appSettingTag, appSetting);
			}
			var variableTag = string.Format("$({0})", tag);
			return contents.Replace(variableTag, value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static void StartProcess()
		{
			_process = Process.Start(ProcessStartInfo());
			WaitForServiceBusToStart();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static ProcessStartInfo ProcessStartInfo()
		{
			var startInfo = new ProcessStartInfo(ConsoleHostExecutablePath())
				{
					UseShellExecute = false,
					RedirectStandardOutput = true
				};
			return startInfo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static void WaitForServiceBusToStart()
		{
			Func<string> readLine = _process.StandardOutput.ReadLine;
			var result = readLine.BeginInvoke(null, null);
			result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(30));
			if (result.IsCompleted)
			{
				var line = readLine.EndInvoke(result);
				if (line.Contains("Service bus is now running"))
					return;
				CloseProcess();
				throw new Exception("Failure starting service bus console host: " + line);
			}
			CloseProcess(); 
			throw new Exception("Timeout starting service bus console host");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static void CloseProcess()
		{
			_process.Kill();
			_process = null;
		}


	}
}