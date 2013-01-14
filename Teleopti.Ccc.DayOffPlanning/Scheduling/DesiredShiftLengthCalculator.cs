﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{
	public interface IDesiredShiftLengthCalculator
	{
		TimeSpan FindAverageLength(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions);
	}

	public class DesiredShiftLengthCalculator : IDesiredShiftLengthCalculator
	{
		private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;

		public DesiredShiftLengthCalculator(ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator)
		{
			_schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public TimeSpan FindAverageLength(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
		{
			var lengths = workShiftMinMaxCalculator.PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions);
			var currentAverage = matrix.SchedulePeriod.AverageWorkTimePerDay;
		    var oldCurrentAverage = currentAverage;
			var targetTime = _schedulePeriodTargetTimeCalculator.TargetTime(matrix);
			if (schedulingOptions.UseCustomTargetTime.HasValue)
				targetTime = schedulingOptions.UseCustomTargetTime.Value;
			var newAverage = TimeSpan.Zero;
			var exit = false;
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
					if (scheduleDay.IsScheduled())
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

				var diff = targetTime.TotalSeconds - fixedTime.TotalSeconds;
				newAverage = TimeSpan.FromSeconds(diff / freeSlots);
				if (newAverage == currentAverage)
					exit = true;
				else
				{
                    if (newAverage == oldCurrentAverage)
                        exit = true;
				    oldCurrentAverage = currentAverage;
					currentAverage = newAverage;
				}
			}

			return newAverage;
		}
	}
}