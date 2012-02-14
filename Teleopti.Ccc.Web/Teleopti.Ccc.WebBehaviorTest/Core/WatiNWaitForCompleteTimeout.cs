using System;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public sealed class WatiNWaitForCompleteTimeout : IDisposable
	{
		private readonly int _waitForCompleteTimeOut;

		public WatiNWaitForCompleteTimeout(int timeoutSeconds)
		{
			_waitForCompleteTimeOut = Settings.WaitForCompleteTimeOut;
			Settings.WaitForCompleteTimeOut = timeoutSeconds;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			Settings.WaitForCompleteTimeOut = _waitForCompleteTimeOut;
		}
	}
}