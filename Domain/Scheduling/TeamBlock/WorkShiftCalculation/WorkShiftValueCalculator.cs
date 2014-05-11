using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
    public interface IWorkShiftValueCalculator
    {
		double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic,
													WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons, TimeZoneInfo timeZoneInfo);
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

        public double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity,
										   IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic,
                                           WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons,
                                           bool useMaximumPersons, TimeZoneInfo timeZoneInfo)
        {
            if (mainShiftLayers == null) throw new ArgumentNullException("mainShiftLayers");
			if (skillIntervalDataDic == null)
                return null;

            double periodValue = 0;
            int resourceInMinutes = 0;

            foreach (IVisualLayer layer in mainShiftLayers)
            {
                var activity = (IActivity)layer.Payload;
                if (activity != skillActivity)
                    continue;

	            var layerPeriod = layer.Period;
				var layerStartLocal = DateTime.SpecifyKind(layerPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var localPeriod = new DateTimePeriod(layerStartLocal, DateTime.SpecifyKind(layerPeriod.EndDateTimeLocal(timeZoneInfo), DateTimeKind.Utc));

				if (!skillIntervalDataDic.Values.Any())
		            return null;

	            int resolution = (int)skillIntervalDataDic.Values.FirstOrDefault().Resolution().TotalMinutes;
				DateTime currentSkillStaffPeriodKey =
					localPeriod.StartDateTime.Date.Add(
						TimeHelper.FitToDefaultResolutionRoundDown(layerStartLocal.TimeOfDay, resolution));

                ISkillIntervalData currentStaffPeriod;
				if (!skillIntervalDataDic.TryGetValue(currentSkillStaffPeriodKey, out currentStaffPeriod))
                {
                    if (activity.RequiresSkill)
                        return null;

                    return 0;
                }

				var currentResourceInMinutes = 0;
				var intersection = currentStaffPeriod.Period.Intersection(localPeriod);
				if (intersection.HasValue)
					currentResourceInMinutes = (int)intersection.Value.ElapsedTime().TotalMinutes;

                while (currentResourceInMinutes > 0)
                {
                    periodValue += _workShiftPeriodValueCalculator.PeriodValue(currentStaffPeriod, currentResourceInMinutes,
                                                                               useMinimumPersons, useMaximumPersons);
                    resourceInMinutes += currentResourceInMinutes;

	                currentSkillStaffPeriodKey = currentSkillStaffPeriodKey.AddMinutes(resolution);
					if(!localPeriod.Contains(currentSkillStaffPeriodKey))
						break;

					if (!skillIntervalDataDic.TryGetValue(currentSkillStaffPeriodKey, out currentStaffPeriod))
						return null;

	                currentResourceInMinutes = 0;
	                intersection = currentStaffPeriod.Period.Intersection(localPeriod);
	                if (intersection.HasValue)
		                currentResourceInMinutes = (int)intersection.Value.ElapsedTime().TotalMinutes;
                }
            }

            return _workShiftLengthValueCalculator.CalculateShiftValueForPeriod(periodValue, resourceInMinutes, lengthFactor);
        }  
    }
}