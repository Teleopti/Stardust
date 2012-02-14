using System;
using System.Threading;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
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