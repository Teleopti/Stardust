using System;
using System.Diagnostics;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class CurrentProcess
	{
		private static readonly Lazy<string> currentProcessName = new Lazy<string>(() => Process.GetCurrentProcess().ProcessName);

		public string Name()
		{
			return currentProcessName.Value;
		}
	}
}