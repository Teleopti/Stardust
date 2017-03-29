using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Teleopti.Support.Tool.AzureStartup
{
	public class StartupScriptRoleEntryPoint : RoleEntryPoint
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(StartupScriptRoleEntryPoint));

		public override bool OnStart()
		{
			XmlConfigurator.Configure();

			logger.Info($"Running OnStart from {AppDomain.CurrentDomain.BaseDirectory}");
			var startupScripts = Environment.GetEnvironmentVariable("RoleStartupScripts");
			if (!string.IsNullOrEmpty(startupScripts))
			{
				var scriptList = startupScripts.Split(';');
				foreach (var script in scriptList)
				{
					logger.Info($"Running script {script}");
					bool waitForExit = false;

					string scriptCommandLine = script;
					if (script.EndsWith("!"))
					{
						scriptCommandLine = script.Substring(0, script.Length - 1);
						waitForExit = true;
					}

					var args = CmdLineToArgvW.SplitArgs(scriptCommandLine);

					var processStartInfo = new ProcessStartInfo
					{
						FileName = args[0],
						Arguments = string.Join(" ", args.Skip(1).ToArray()),
						WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "bin"),
						UseShellExecute = true,
					};
					try
					{
						var process = Process.Start(processStartInfo);
						if (waitForExit)
						{
							process.WaitForExit();
						}
					}
					catch (Exception e)
					{
						logger.Error($"Failed while running {script}", e);
						throw;
					}
				}
			}
			return base.OnStart();
		}

		internal static class CmdLineToArgvW
		{
			// The previous examples on this page used incorrect
			// pointer logic and were removed.

			internal static string[] SplitArgs(string unsplitArgumentLine)
			{
				int numberOfArgs;

				var ptrToSplitArgs = CommandLineToArgvW(unsplitArgumentLine, out numberOfArgs);

				// CommandLineToArgvW returns NULL upon failure.
				if (ptrToSplitArgs == IntPtr.Zero)
					throw new ArgumentException("Unable to split argument.", new Win32Exception());

				// Make sure the memory ptrToSplitArgs to is freed, even upon failure.
				try
				{
					var splitArgs = new string[numberOfArgs];

					// ptrToSplitArgs is an array of pointers to null terminated Unicode strings.
					// Copy each of these strings into our split argument array.
					for (int i = 0; i < numberOfArgs; i++)
						splitArgs[i] = Marshal.PtrToStringUni(
							Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size));

					return splitArgs;
				}
				finally
				{
					// Free memory obtained by CommandLineToArgW.
					LocalFree(ptrToSplitArgs);
				}
			}

			[DllImport("shell32.dll", SetLastError = true)]
			static extern IntPtr CommandLineToArgvW(
				[MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
				out int pNumArgs);

			[DllImport("kernel32.dll")]
			static extern IntPtr LocalFree(IntPtr hMem);
		}
	}
}