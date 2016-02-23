using System;
using System.Timers;

namespace Stardust.Node.Extensions
{
	public static class TimerExtensions
	{
		public static void ThrowArgumentNullExceptionWhenNull(this Timer timer)
		{
			if (timer == null)
			{
				throw new ArgumentNullException("timer");
			}
		}
	}
}