using System;
using System.Diagnostics;
using System.IO;
using IISExpressAutomation;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public class IisExpressRunner : IDisposable
	{
		private Process _process;

		public void Execute(Parameters parameters, string iisExpressPath = @"C:\Program Files (x86)\IIS Express\iisexpress.exe")
		{
			if (!File.Exists(iisExpressPath))
			{
				throw new ArgumentException("IIS Express executable not found", iisExpressPath);
			}

			var info = new ProcessStartInfo
			{
				FileName = iisExpressPath,
				Arguments = parameters?.ToString() ?? "",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			_process = new Process {StartInfo = info};
			_process.OutputDataReceived += (sender, args) => TestLog.Static.Debug(args.Data);
			_process.ErrorDataReceived += (sender, args) => TestLog.Static.Debug(args.Data);

			_process.Start();
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();
		}

		public void Dispose()
		{
			_process.Dispose();
		}
	}
}
