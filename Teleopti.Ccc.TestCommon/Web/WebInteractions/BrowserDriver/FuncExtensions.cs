using System;
using System.Threading;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public static class FuncExtensions
	{
		public static bool WaitUntil(this Func<bool> instance, TimeSpan pollTime, TimeSpan timeOut)
		{
			var timeOutTime = DateTime.Now.Add(timeOut);
			while (!instance.Invoke())
			{
				if (DateTime.Now > timeOutTime)
					return false;
				Thread.Sleep(pollTime);
			}
			return true;
		}
	}
}