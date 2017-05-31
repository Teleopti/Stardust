using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISchedulingSourceScope
	{
		IDisposable OnThisThreadUse(string schedulingScope);
	}

	public class ScheduleSource
	{
		public static readonly string WebScheduling = "WebScheduling";
	}
}