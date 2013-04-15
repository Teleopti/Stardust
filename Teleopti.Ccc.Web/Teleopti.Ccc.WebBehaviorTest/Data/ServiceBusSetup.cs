using System;
using System.Collections.Generic;
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
			if (!IniFileInfo.ServiceBus)
				return;
			Configure();
			StartProcess();
		}

		public static void TearDown()
		{
			if (!IniFileInfo.ServiceBus)
				return;
			CloseProcess();
		}



		private static void Configure()
		{
			var additionalTags = new Dictionary<string, string> {{"MessagesOnBoot", "false"}};
			FileConfigurator.ConfigureByTags(
				ConsoleHostConfigSourcePath(),
				ConsoleHostConfigTargetPath(),
				new AllTags(additionalTags)
				);
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