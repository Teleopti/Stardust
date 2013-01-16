using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IWorkShiftValueCalculator
	{
		double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDatas,
		                                            WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons, double overStaffingFactor, double priorityFactor);
	}

	public class WorkShiftValueCalculator : IWorkShiftValueCalculator
	{
		private readonly IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
		private readonly IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;

		public WorkShiftValueCalculator(IWorkShiftPeriodValueCalculator workShiftPeriodValueCalculator, IWorkShiftLengthValueCalculator workShiftLengthValueCalculator)
		{
			_workShiftPeriodValueCalculator = workShiftPeriodValueCalculator;
			_workShiftLengthValueCalculator = workShiftLengthValueCalculator;
		}

		public double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDatas,
		  WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons, double overStaffingFactor, double priorityFactor)
		{
			if (skillIntervalDatas.Count == 0)
				return null;

			double periodValue = 0;
			int resourceInMinutes = 0;
			DateTime shiftStartDate = mainShiftLayers.Period().Value.StartDateTime.Date;
			foreach (IVisualLayer layer in mainShiftLayers)
			{
				IActivity activity = (IActivity) layer.Payload;
				if (activity != skillActivity)
					continue;

				DateTime layerStart = layer.Period.StartDateTime;
				DateTime layerEnd = layer.Period.EndDateTime;

				int resolution = getResolution(skillIntervalDatas);
				int currentResourceInMinutes = resolution;

				DateTime currentStart =
					layerStart.Date.Add(
						TimeHelper.FitToDefaultResolution(layerStart.TimeOfDay, resolution));
				if (currentStart > layerStart)
				{
					currentStart = currentStart.AddMinutes(-resolution);
				}
				// If the layer doesn't fit to the resolution (maybe a short break of 5 minutes in the start)
				if (currentStart < layerStart)
				{
					currentResourceInMinutes = currentResourceInMinutes - (layerStart - currentStart).Minutes;
				}

				// IF the shift is outside opening hours and Activity needs skill dont't use it (otherwise it could be outside opening hours).
				ISkillIntervalData currentStaffPeriod;
				if (!skillIntervalDatas.TryGetValue(timeOfDay(shiftStartDate, currentStart), out currentStaffPeriod))
				{
					if (activity.RequiresSkill)
						return null;
				}

				// only part of the period should count
				if (currentResourceInMinutes > 0 && currentStaffPeriod.Period.EndDateTime > layerEnd)
					currentResourceInMinutes = currentResourceInMinutes - (currentStaffPeriod.Period.EndDateTime - layerEnd).Minutes;

				while (currentResourceInMinutes > 0)
				{
					periodValue += _workShiftPeriodValueCalculator.PeriodValue(currentStaffPeriod, currentResourceInMinutes,
					                                                           useMinimumPersons, useMaximumPersons, overStaffingFactor,
					                                                           priorityFactor);
					resourceInMinutes += currentResourceInMinutes;

					currentResourceInMinutes = resolution;
					currentStart = currentStart.AddMinutes(resolution);

					if (currentStart >= layer.Period.EndDateTime)
					{
						break;
					}

					if (!skillIntervalDatas.TryGetValue(timeOfDay(shiftStartDate, currentStart), out currentStaffPeriod))
					{
						if (activity.RequiresSkill)
							return null;
					}

					// only part of the period should count
					if (currentResourceInMinutes > 0 && currentStaffPeriod.Period.EndDateTime > layerEnd)
						currentResourceInMinutes = currentResourceInMinutes - (currentStaffPeriod.Period.EndDateTime - layerEnd).Minutes;
				}
			}

			return _workShiftLengthValueCalculator.CalculateShiftValueForPeriod(periodValue, resourceInMinutes, lengthFactor);
		}

		private static int getResolution(IDictionary<TimeSpan, ISkillIntervalData> staffPeriods)
		{
			int resolution = 15;
			foreach (KeyValuePair<TimeSpan, ISkillIntervalData> pair in staffPeriods)
			{
				resolution = (int)((pair.Value.Period.EndDateTime - pair.Value.Period.StartDateTime).TotalMinutes);
				break;
			}
			return resolution;
		}

		private TimeSpan timeOfDay(DateTime shiftStartDate, DateTime currentStart)
		{
			if (currentStart.Date == shiftStartDate.Date)
				return currentStart.TimeOfDay;

			return currentStart.TimeOfDay.Add(TimeSpan.FromDays(1));
		}
	}
}