using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	//extracted from schedulestorage
	public static class ScheduleDateTimePeriodExtensions
	{
		public static DateOnlyPeriod LongLoadedDateOnlyPeriod(this IScheduleDateTimePeriod scheduleDateTimePeriod)
		{
			return extendPeriod(scheduleDateTimePeriod.LoadedPeriod());
		}

		public static DateOnlyPeriod LongVisibleDateOnlyPeriod(this IScheduleDateTimePeriod scheduleDateTimePeriod)
		{
			return extendPeriod(scheduleDateTimePeriod.VisiblePeriod);
		}

		private static DateOnlyPeriod extendPeriod(DateTimePeriod period)
		{
			return new DateOnlyPeriod(new DateOnly(period.StartDateTime.AddDays(-1)), new DateOnly(period.EndDateTime.AddDays(1)));
		}
	}
}