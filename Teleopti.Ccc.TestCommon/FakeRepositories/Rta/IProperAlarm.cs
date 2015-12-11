using System;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public interface IProperAlarm
	{
		bool IsAlarm(TimeSpan? threshold);
	}

	public class ProperAlarmDisabled : IProperAlarm
	{
		public bool IsAlarm(TimeSpan? threshold)
		{
			return false;
		}
	}

	public class ProperAlarmEnabled : IProperAlarm
	{
		public bool IsAlarm(TimeSpan? threshold)
		{
			return threshold != null;
		}
	}
}