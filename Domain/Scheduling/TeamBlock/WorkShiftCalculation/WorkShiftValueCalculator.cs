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
		double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic, PeriodValueCalculationParameters periodValueCalculationParameters, TimeZoneInfo timeZoneInfo, IDictionary<DateTime, bool> maxSeatsPerIntervalDictionary);
	}

	public class WorkShiftValueCalculator : IWorkShiftValueCalculator
	{
        private readonly IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
        private readonly IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
	    private readonly IMaxSeatsCalculationForTeamBlock _maxSeatsCalculationForTeamBlock;

	    public WorkShiftValueCalculator(IWorkShiftPeriodValueCalculator workShiftPeriodValueCalculator, IWorkShiftLengthValueCalculator workShiftLengthValueCalculator, IMaxSeatsCalculationForTeamBlock maxSeatsCalculationForTeamBlock)
        {
            _workShiftPeriodValueCalculator = workShiftPeriodValueCalculator;
            _workShiftLengthValueCalculator = workShiftLengthValueCalculator;
	        _maxSeatsCalculationForTeamBlock = maxSeatsCalculationForTeamBlock;
        }

		public double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity, IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic, PeriodValueCalculationParameters periodValueCalculationParameters, TimeZoneInfo timeZoneInfo, IDictionary<DateTime, bool> maxSeatsPerIntervalDictionary)
		{
			if (mainShiftLayers == null) throw new ArgumentNullException("mainShiftLayers");
			if (skillIntervalDataDic == null)
				return null;

			double periodValue = 0;
			int resourceInMinutes = 0;

			foreach (IVisualLayer layer in mainShiftLayers)
			{
				var activity = (IActivity) layer.Payload;
				if (activity != skillActivity)
					continue;

				var layerPeriod = layer.Period;
				var layerStartLocal = DateTime.SpecifyKind(layerPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var localPeriod = new DateTimePeriod(layerStartLocal,
					DateTime.SpecifyKind(layerPeriod.EndDateTimeLocal(timeZoneInfo), DateTimeKind.Utc));

				if (!skillIntervalDataDic.Values.Any())
					return null;

				int resolution = (int) skillIntervalDataDic.Values.FirstOrDefault().Resolution().TotalMinutes;
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

				bool maxSeatReached = false;
				if (!maxSeatsPerIntervalDictionary.TryGetValue(currentSkillStaffPeriodKey, out maxSeatReached))
				{
					//the following code should be used but for now we will just use this value
					maxSeatReached = false;
					//if (activity.RequiresSeat )
					//	return null;

					//return 0;
				}

				var currentResourceInMinutes = 0;
				var intersection = currentStaffPeriod.Period.Intersection(localPeriod);
				if (intersection.HasValue)
					currentResourceInMinutes = (int) intersection.Value.ElapsedTime().TotalMinutes;

				while (currentResourceInMinutes > 0)
				{
					var valueForThisPeriod = _workShiftPeriodValueCalculator.PeriodValue(currentStaffPeriod, currentResourceInMinutes,
						periodValueCalculationParameters.UseMinimumPersons, periodValueCalculationParameters.UseMaximumPersons);
					var maxSeatsCorrection = _maxSeatsCalculationForTeamBlock.PeriodValue(valueForThisPeriod,
						MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, maxSeatReached, activity.RequiresSeat);

					if (maxSeatsCorrection == null)
						return null;
					periodValue += maxSeatsCorrection.Value;
					resourceInMinutes += currentResourceInMinutes;

					currentSkillStaffPeriodKey = currentSkillStaffPeriodKey.AddMinutes(resolution);
					if (!localPeriod.Contains(currentSkillStaffPeriodKey))
						break;

					if (!skillIntervalDataDic.TryGetValue(currentSkillStaffPeriodKey, out currentStaffPeriod))
						return null;

					currentResourceInMinutes = 0;
					intersection = currentStaffPeriod.Period.Intersection(localPeriod);
					if (intersection.HasValue)
						currentResourceInMinutes = (int) intersection.Value.ElapsedTime().TotalMinutes;
				}
			}

			return _workShiftLengthValueCalculator.CalculateShiftValueForPeriod(periodValue, resourceInMinutes,
				periodValueCalculationParameters.LengthFactor);
		}
	}
}