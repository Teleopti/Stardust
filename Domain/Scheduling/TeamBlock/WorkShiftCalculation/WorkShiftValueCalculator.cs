using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IContainingSkillIntervalPeriodFinder
	{
		DateTime? Find(IDictionary<DateTime, ISkillIntervalData> skillIntervals, DateTime periodKey, int resolution);
	}

	public class ContainingSkillIntervalPeriodFinder : IContainingSkillIntervalPeriodFinder
	{
		public DateTime? Find(IDictionary<DateTime, ISkillIntervalData> skillIntervals, DateTime periodKey, int resolution)
		{
			if (resolution < 60) return null;
			foreach (var skillIntervalData in skillIntervals)
			{
				if (!skillIntervalData.Value.Period.Contains(periodKey)) continue;
				return skillIntervalData.Key;
			}

			return null;
		}
	}
		
	public class WorkShiftValueCalculator
	{
		private readonly IWorkShiftPeriodValueCalculator _workShiftPeriodValueCalculator;
		private readonly IWorkShiftLengthValueCalculator _workShiftLengthValueCalculator;
		private readonly IContainingSkillIntervalPeriodFinder _containingSkillIntervalPeriodFinder;

		public WorkShiftValueCalculator(IWorkShiftPeriodValueCalculator workShiftPeriodValueCalculator,
			IWorkShiftLengthValueCalculator workShiftLengthValueCalculator,
			IContainingSkillIntervalPeriodFinder containingSkillIntervalPeriodFinder)
		{
			_workShiftPeriodValueCalculator = workShiftPeriodValueCalculator;
			_workShiftLengthValueCalculator = workShiftLengthValueCalculator;
			_containingSkillIntervalPeriodFinder = containingSkillIntervalPeriodFinder;
		}

		public double? CalculateShiftValue(IVisualLayerCollection mainShiftLayers, IActivity skillActivity,
			IDictionary<DateTime, ISkillIntervalData> skillIntervalDataDic,
			PeriodValueCalculationParameters periodValueCalculationParameters, TimeZoneInfo timeZoneInfo)
		{
			if (mainShiftLayers == null) throw new ArgumentNullException(nameof(mainShiftLayers));
			if (skillIntervalDataDic == null)
				return null;

			var firstSkillIntervalData = skillIntervalDataDic.Values.FirstOrDefault();
			int? resolution = firstSkillIntervalData!=null ? (int)firstSkillIntervalData.Resolution().TotalMinutes : (int?)null;
			
			double periodValue = 0;
			int resourceInMinutes = 0;

			foreach (IVisualLayer layer in mainShiftLayers.AsParallel())
			{
				var activity = (IActivity) layer.Payload;
				if (!activity.Equals(skillActivity))
					continue;

				if (!resolution.HasValue) return null;

				var layerPeriod = layer.Period;
				var layerStartLocal = DateTime.SpecifyKind(layerPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				var localPeriod = new DateTimePeriod(layerStartLocal, layerStartLocal.Add(layerPeriod.ElapsedTime()));

				var currentSkillStaffPeriodKey =
					localPeriod.StartDateTime.Date.Add(
						TimeHelper.FitToDefaultResolutionRoundDown(layerStartLocal.TimeOfDay, resolution.Value));

				ISkillIntervalData currentStaffPeriod;
				if (!skillIntervalDataDic.TryGetValue(currentSkillStaffPeriodKey, out currentStaffPeriod))
				{
					var containingPeriodKey = _containingSkillIntervalPeriodFinder.Find(skillIntervalDataDic, currentSkillStaffPeriodKey, resolution.Value);
					if (containingPeriodKey.HasValue)
					{
						currentSkillStaffPeriodKey = containingPeriodKey.Value;
						currentStaffPeriod = skillIntervalDataDic[containingPeriodKey.Value];
					}
					else
					{
						if (activity.RequiresSkill)
							return null;

						return 0;
					}	
				}

				var currentResourceInMinutes = 0;
				var intersection = currentStaffPeriod.Period.Intersection(localPeriod);
				if (intersection.HasValue)
					currentResourceInMinutes = (int) intersection.Value.ElapsedTime().TotalMinutes;

				while (currentResourceInMinutes > 0)
				{
					var valueForThisPeriod = _workShiftPeriodValueCalculator.PeriodValue(currentStaffPeriod, currentResourceInMinutes,
						periodValueCalculationParameters.UseMinimumPersons, periodValueCalculationParameters.UseMaximumPersons);
					periodValue += valueForThisPeriod;
					
					resourceInMinutes += currentResourceInMinutes;

					currentSkillStaffPeriodKey = currentSkillStaffPeriodKey.AddMinutes(resolution.Value);
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