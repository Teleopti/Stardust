﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class ScheduleChangeCallbackForMaxSeatOptimization : IScheduleDayChangeCallback
	{
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;

		public ScheduleChangeCallbackForMaxSeatOptimization(IResourceCalculateDaysDecider resourceCalculateDaysDecider)
		{
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			ModifiedDates = new HashSet<DateOnly>();
		}

		public HashSet<DateOnly> ModifiedDates { get;}

		public void ScheduleDayBeforeChanging()
		{
			ResourceCalculationContext.Fetch();
		}

		public void ScheduleDayChanged(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			applyChangesToResourceContainer(partBefore, partAfter);
			markModifiedDates(partBefore, partAfter);
		}

		private static void applyChangesToResourceContainer(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			var container = ResourceCalculationContext.Fetch();
			container.RemoveScheduleDayFromContainer(partBefore, container.MinSkillResolution);
			container.AddScheduleDayToContainer(partAfter, container.MinSkillResolution);
		}

		private void markModifiedDates(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			_resourceCalculateDaysDecider.DecideDates(partAfter, partBefore).ForEach(x => ModifiedDates.Add(x));
		}
	}
}