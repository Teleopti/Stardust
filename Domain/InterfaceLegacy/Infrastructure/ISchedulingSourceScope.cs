using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
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