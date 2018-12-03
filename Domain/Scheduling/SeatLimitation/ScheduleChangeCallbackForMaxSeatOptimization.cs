using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class ScheduleChangeCallbackForMaxSeatOptimization : IScheduleDayChangeCallback
	{
		private readonly ScheduleChangesAffectedDates _resourceCalculateDaysDecider;

		public ScheduleChangeCallbackForMaxSeatOptimization(ScheduleChangesAffectedDates resourceCalculateDaysDecider)
		{
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			ModifiedDates = new HashSet<DateOnly>();
		}

		public HashSet<DateOnly> ModifiedDates { get;}

		public void ScheduleDayBeforeChanging()
		{
			ResourceCalculationContext.Fetch();
		}

		public void ScheduleDayChanged(IScheduleDay partBefore)
		{
			var partAfter = partBefore.ReFetch();
			applyChangesToResourceContainer(partBefore, partAfter);
			markModifiedDates(partBefore, partAfter);
		}

		private static void applyChangesToResourceContainer(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			var container = ResourceCalculationContext.Fetch();
			container.RemoveScheduleDayFromContainer(partBefore);
			container.AddScheduleDayToContainer(partAfter);
		}

		private void markModifiedDates(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			_resourceCalculateDaysDecider.DecideDates(partAfter, partBefore).ForEach(x => ModifiedDates.Add(x));
		}
	}
}