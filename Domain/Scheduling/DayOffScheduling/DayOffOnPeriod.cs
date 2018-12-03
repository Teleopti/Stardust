using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public class DayOffOnPeriod
	{
		private readonly IList<IScheduleDay> _scheduleDays;

		public DayOffOnPeriod(DateOnlyPeriod period, IList<IScheduleDay> scheduleDays, int dayOffsCount)
		{
			_scheduleDays = scheduleDays;
			Period = period;
			DaysOffCount = dayOffsCount;
		}

		public DateOnlyPeriod Period { get; }

		public int DaysOffCount { get; }

		public IScheduleDay FindBestSpotForDayOff(IHasContractDayOffDefinition hasContractDayOffDefinition, IScheduleDayAvailableForDayOffSpecification dayAvailableForDayOffSpecification, IEffectiveRestrictionCreator effectiveRestrictionCreator, SchedulingOptions schedulingOptions)
		{
			var contractDayOffs = new List<IScheduleDay>();
			var bestSpotList = new List<Tuple<IScheduleDay, int>>();

			var allContractDayOffIsSatisfied = true;

			foreach (var scheduleDay in _scheduleDays)
			{
				if (hasContractDayOffDefinition.IsDayOff(scheduleDay))
				{
					if (!scheduleDay.HasDayOff()) allContractDayOffIsSatisfied = false;
					contractDayOffs.Add(scheduleDay);
				}
			}

			if (allContractDayOffIsSatisfied) return null;

			foreach (var scheduleDay in _scheduleDays)
			{
				if (contractDayOffs.Count == 0) break;
				if (!dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay)) continue;
				var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
				if (effectiveRestriction != null && effectiveRestriction.NotAllowedForDayOffs) continue;	

				var minDiff = int.MaxValue;

				foreach (var contractDayOff in contractDayOffs)
				{
					var diff = Math.Abs(scheduleDay.DateOnlyAsPeriod.DateOnly.Date.Subtract(contractDayOff.DateOnlyAsPeriod.DateOnly.Date).TotalDays);
					if (diff < minDiff)
						minDiff = (int)diff;
				}

				var kvp = new Tuple<IScheduleDay, int>(scheduleDay, minDiff);
				bestSpotList.Add(kvp);
			}

			if (bestSpotList.Count == 0) return null;

			var bestKvp = bestSpotList.OrderBy(kvp => kvp.Item2).ThenByDescending(kvp => kvp.Item1.DateOnlyAsPeriod.DateOnly.Date).First();
			return bestKvp.Item1;
		}
	}
}
