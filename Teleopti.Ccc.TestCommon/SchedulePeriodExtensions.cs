using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class SchedulePeriodExtensions
	{
		public static ISchedulePeriod NumberOfDaysOff(this ISchedulePeriod schedulePeriod, int numberOfDaysOff)
		{
			schedulePeriod.SetDaysOff(numberOfDaysOff);
			return schedulePeriod;
		}
	}
}