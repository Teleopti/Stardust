using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftValueCalculator
	{
		double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDataList,
		                                            WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons);
	}

	public class WorkShiftValueCalculator : IWorkShiftValueCalculator
	{
		private readonly IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
		private readonly IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
		private readonly IVisualLayerToBaseDateMapper _visualLayerToBaseDateMapper;

		[CLSCompliant(false)]
		public WorkShiftValueCalculator(IWorkShiftPeriodValueCalculator workShiftPeriodValueCalculator, IWorkShiftLengthValueCalculator workShiftLengthValueCalculator, IVisualLayerToBaseDateMapper visualLayerToBaseDateMapper)
		{
			_workShiftPeriodValueCalculator = workShiftPeriodValueCalculator;
			_workShiftLengthValueCalculator = workShiftLengthValueCalculator;
			_visualLayerToBaseDateMapper = visualLayerToBaseDateMapper;
		}

		public double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity,
		                                   IDictionary<TimeSpan, ISkillIntervalData> skillIntervalDataList,
		                                   WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons,
		                                   bool useMaximumPersons)
		{
		    if (mainShiftLayers == null) 
				throw new ArgumentNullException("mainShiftLayers");

		    if (skillIntervalDataList == null)
				return null;

			double periodValue = 0;
			int resourceInMinutes = 0;
			IVisualLayer firstLayerInTheShift = null;
			foreach (IVisualLayer layer in mainShiftLayers)
			{
				if (firstLayerInTheShift == null)
					firstLayerInTheShift = layer;

				var activity = (IActivity) layer.Payload;
				if (activity != skillActivity)
					continue;

				int resolution = getResolution(skillIntervalDataList);
				var currentSkillIntervalDataKey = _visualLayerToBaseDateMapper.Map(layer, firstLayerInTheShift, resolution);

				ISkillIntervalData currentSkillIntervalData;
				if (!skillIntervalDataList.TryGetValue(currentSkillIntervalDataKey, out currentSkillIntervalData))
				{
					if (activity.RequiresSkill)
						return null;

					return 0;
				}

				var currentResourceInMinutes =
					(int)currentSkillIntervalData.Period.Intersection(layer.Period).Value.ElapsedTime().TotalMinutes;

				
				//do the simplest possible loop here
					while (currentResourceInMinutes > 0)
					{
						periodValue += _workShiftPeriodValueCalculator.PeriodValue(currentSkillIntervalData, currentResourceInMinutes,
						                                                           useMinimumPersons, useMaximumPersons);

						currentSkillIntervalDataKey = currentSkillIntervalDataKey.Add(TimeSpan.FromMinutes(resolution));

						if (!skillIntervalDataList.TryGetValue(currentSkillIntervalDataKey, out currentSkillIntervalData))
						{
							if (activity.RequiresSkill)
								return null;

							return 0;
						}

						currentResourceInMinutes = (int)currentSkillIntervalData.Period.Intersection(layer.Period).Value.ElapsedTime().TotalMinutes;

					}
				
			}

			return _workShiftLengthValueCalculator.CalculateShiftValueForPeriod(periodValue, resourceInMinutes, lengthFactor);
		}

		private static int getResolution(IEnumerable<KeyValuePair<TimeSpan, ISkillIntervalData>> staffPeriods)
		{
			int resolution = 15;
			foreach (KeyValuePair<TimeSpan, ISkillIntervalData> pair in staffPeriods)
			{
				resolution = (int)(pair.Value.Resolution().TotalMinutes);
				break;
			}
			return resolution;
		}

		private static TimeSpan timeOfDay(DateTime shiftStartDate, DateTime currentStart)
		{
			if (currentStart.Date == shiftStartDate.Date)
				return currentStart.TimeOfDay;

			return currentStart.TimeOfDay.Add(TimeSpan.FromDays(1));
		}

		private static IVisualLayer layerToBaseDate(IVisualLayer layer, DateTime shiftStartDate)
		{
			var movedStart =
				new DateTime(SkillDayTemplate.BaseDate.Date.Ticks, DateTimeKind.Utc).Add(timeOfDay(shiftStartDate,
				                                                                                   layer.Period.StartDateTime));
			var movedEnd =
				new DateTime(SkillDayTemplate.BaseDate.Date.Ticks, DateTimeKind.Utc).Add(timeOfDay(shiftStartDate,
				                                                                                   layer.Period.EndDateTime));
			var
			movedPeriod = new DateTimePeriod(movedStart, movedEnd);
			var retLayer = new VisualLayer(layer.Payload, movedPeriod, ((VisualLayer) layer).HighestPriorityActivity,
			                               layer.Person);
			return retLayer;
		}
	}
}