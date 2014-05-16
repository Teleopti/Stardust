using System;
using System.Threading;

namespace Teleopti.Interfaces.Domain
{
	public interface ITime
	{
		DateTime UtcDateTime();
		object StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
		void DisposeTimer(object timer);
	}
}