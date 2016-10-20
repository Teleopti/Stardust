using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IExtractIntervalsViolatingMaxSeat
	{
		IDictionary<DateTime , IntervalLevelMaxSeatInfo> IdentifyIntervalsWithBrokenMaxSeats(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder, TimeZoneInfo timeZone, DateOnly baseDatePointer);
	}

	public class ExtractIntervalsViolatingMaxSeat : IExtractIntervalsViolatingMaxSeat
	{
		private readonly IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;

		public ExtractIntervalsViolatingMaxSeat(
			IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals)
		{
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
		}

		public IDictionary<DateTime, IntervalLevelMaxSeatInfo> IdentifyIntervalsWithBrokenMaxSeats(ITeamBlockInfo teamBlockInfo, ISchedulingResultStateHolder schedulingResultStateHolder, TimeZoneInfo timeZone, DateOnly baseDatePointer)
		{
			var aggregatedIntervalsForTheWholeBlockNew = new Dictionary<DateTime , IntervalLevelMaxSeatInfo>();
			var allSkillDays = schedulingResultStateHolder.AllSkillDays();
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				var eachDayIntervals = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, dateOnly, allSkillDays, timeZone,false);
				mergeIntervalsWithMaxSeatInformation(eachDayIntervals, aggregatedIntervalsForTheWholeBlockNew,baseDatePointer);
			}
			//get the intervals of last day 
			var lastNextDay = baseDatePointer.AddDays(1);
			var lastNextDayIntervals = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, teamBlockInfo.BlockInfo.BlockPeriod.EndDate.AddDays(1), allSkillDays, timeZone,false);
			mergeIntervalsWithMaxSeatInformation(lastNextDayIntervals, aggregatedIntervalsForTheWholeBlockNew, lastNextDay);
			//
			return aggregatedIntervalsForTheWholeBlockNew;
		}

		private void mergeIntervalsWithMaxSeatInformation(IDictionary<DateTime, IntervalLevelMaxSeatInfo> eachDayIntervals, IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedIntervalsForTheWholeBlock, DateOnly baseDatePointer)
		{
			foreach (var newInterval in eachDayIntervals)
			{
				var newIntervalKey = newInterval.Key.TimeOfDay;
				var newIntervalValue = newInterval.Value;
				if (newIntervalValue.IsMaxSeatReached)
				{
					IntervalLevelMaxSeatInfo info;
					if (aggregatedIntervalsForTheWholeBlock.TryGetValue(baseDatePointer.Date.Add(newIntervalKey), out info))
					{
						if (info.IsMaxSeatReached)
						{
							var aggregatedInterval = new IntervalLevelMaxSeatInfo(newIntervalValue.IsMaxSeatReached,
								newIntervalValue.MaxSeatBoostingFactor +
								info.MaxSeatBoostingFactor);
							aggregatedIntervalsForTheWholeBlock[baseDatePointer.Date.Add(newIntervalKey)] = aggregatedInterval;
						}
						else
							aggregatedIntervalsForTheWholeBlock[baseDatePointer.Date.Add(newIntervalKey)] = newIntervalValue;
					}
					else
					{
							aggregatedIntervalsForTheWholeBlock.Add(baseDatePointer.Date.Add(newIntervalKey), newIntervalValue);
					}
				}
				else
				{
					if (aggregatedIntervalsForTheWholeBlock.ContainsKey(baseDatePointer.Date.Add(newIntervalKey)))
					{
						if(newIntervalValue.IsMaxSeatReached)
							aggregatedIntervalsForTheWholeBlock[baseDatePointer.Date.Add(newIntervalKey)] = newIntervalValue;
					}
					else
						aggregatedIntervalsForTheWholeBlock.Add(baseDatePointer.Date.Add(newIntervalKey), newIntervalValue);
				}
					
			}
		}
	}
}