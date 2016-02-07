using System;
using System.Threading;

namespace Teleopti.Interfaces.Domain
{
	public interface ITime
	{
		DateTime UtcDateTime();
		IDisposable StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
	}
}