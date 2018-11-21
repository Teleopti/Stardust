using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
	public interface IDesiredShiftLengthCalculator
	{
		TimeSpan FindAverageLength(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, IDictionary<DateOnly, TimeSpan> maxWorkTimeDictionary);
	}

	public class DesiredShiftLengthCalculator : IDesiredShiftLengthCalculator
	{
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public DesiredShiftLengthCalculator(ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		public TimeSpan FindAverageLength(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, IDictionary<DateOnly, TimeSpan> maxWorkTimeDictionary)
		{
			workShiftMinMaxCalculator.ResetCache();
			var lengths = workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions, maxWorkTimeDictionary);
			var currentAverage = matrix.SchedulePeriod.AverageWorkTimePerDay;
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTime(matrix);
			if (schedulingOptions.UseCustomTargetTime.HasValue)
				targetTime = schedulingOptions.UseCustomTargetTime.Value;
			var newAverage = TimeSpan.Zero;
			var exit = false;
			var candidateAverages = new HashSet<TimeSpan>();
			while (!exit)
			{
				var fixedTime = TimeSpan.Zero;
				var freeSlots = 0;
				foreach (var keyValuePair in lengths)
				{
					if (!matrix.SchedulePeriod.DateOnlyPeriod.Contains(keyValuePair.Key))
						continue;

					//ersätt schemalagda dagar och räkna slots
					IScheduleDay scheduleDay = matrix.GetScheduleDayByKey(keyValuePair.Key).DaySchedulePart();
					if (schedulingOptions.IsDayScheduled(scheduleDay))
					{
						fixedTime = fixedTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
						continue;
					}

					if (currentAverage < keyValuePair.Value.Minimum)
					{
						fixedTime = fixedTime.Add(keyValuePair.Value.Minimum);
						continue;
					}

					if (currentAverage > keyValuePair.Value.Maximum)
					{
						fixedTime = fixedTime.Add(keyValuePair.Value.Maximum);
						continue;
					}

					freeSlots++;
				}

				if (freeSlots == 0)
					return currentAverage;

				var diff = targetTime - fixedTime;
				newAverage = TimeSpan.FromSeconds(diff.TotalSeconds / freeSlots);
				candidateAverages.Add(newAverage);
				if (newAverage == currentAverage)
					exit = true;
				else
				{
					if (candidateAverages.Contains(currentAverage))
						exit = true;

					currentAverage = newAverage;
				}
			}

			return newAverage;
		}
	}
}