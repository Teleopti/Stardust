using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HalfHourSkillTimeZone_75509)]
	public class ScheduleResourcePeriodFetcher
	{
		public virtual DateTimePeriod Fetch(DateTimePeriod period, ISkill skill)
		{
			return period;
		}
	}

	[RemoveMeWithToggle("use as function instead", Toggles.ResourcePlanner_HalfHourSkillTimeZone_75509)]
	public class ScheduleResourcePeriodFetcherAdjustForTimeZone : ScheduleResourcePeriodFetcher
	{
		public override DateTimePeriod Fetch(DateTimePeriod period, ISkill skill)
		{
			var minutesOffset = skill.TimeZone.BaseUtcOffset.Minutes;
			if (minutesOffset == 0)
				return period;

			minutesOffset = 60 - minutesOffset;
			if (minutesOffset > 60)
				minutesOffset = minutesOffset % 60 * -1;
			
			return period.MovePeriod(TimeSpan.FromMinutes(minutesOffset));
		}
	}

	/// <summary>
	/// Takes the input domain classes, runs the resource optimalization and writes back the result
	/// data to the domain.
	/// </summary>
	public class ScheduleResourceOptimizer
	{
		private readonly IResourceCalculationDataContainer _relevantProjections;
		private readonly ISkillResourceCalculationPeriodDictionary _skillStaffPeriods;
		private readonly IAffectedPersonSkillService _personSkillService;
		private readonly IActivityDivider _activityDivider;
		private readonly IList<IActivity> _distinctActivities;

		private const double _quotient = 1d; // the outer quotient: default = 1
		private const int _maximumIteration = 100; // the maximum number of iterations
		private ScheduleResourcePeriodFetcher _scheduleResourcePeriodFetcher;

		public ScheduleResourceOptimizer(IResourceCalculationDataContainer relevantProjections,
										 ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods,
										 IAffectedPersonSkillService personSkillService,
										 bool clearSkillStaffPeriods, IActivityDivider activityDivider)
		{
			_relevantProjections = relevantProjections;
			_skillStaffPeriods = relevantSkillStaffPeriods;
			_personSkillService = personSkillService;
			_activityDivider = activityDivider;
			_distinctActivities = getDistinctActivities();
			if (clearSkillStaffPeriods)
				this.clearSkillStaffPeriods();
			_scheduleResourcePeriodFetcher = ServiceLocatorForLegacy.ScheduleResourcePeriodFetcher;
		}

		public void Optimize(DateTimePeriod datePeriodToRecalculate, ResourceCalculationData resourceCalculationData = null)
		{
			var affectedSkills = _personSkillService.AffectedSkills;
			_distinctActivities.ForEach(
				currentActivity =>
						optimizeActivity(affectedSkills, currentActivity, datePeriodToRecalculate, resourceCalculationData));
		}

		private void optimizeActivity(IEnumerable<ISkill> affectedSkills, IActivity currentActivity, DateTimePeriod datePeriodToRecalculate, ResourceCalculationData resourceCalculationData)
		{
			IList<ISkill> skills = new List<ISkill>();
			foreach (var affectedSkill in affectedSkills)
			{
				if (affectedSkill != null && affectedSkill.Activity.Equals(currentActivity))
					skills.Add(affectedSkill);
			}

			//All skills with same activity must have the same resolution
			var defaultResolution = TimeSpan.FromMinutes(skills[0].DefaultResolution);
			var currentStart =
				datePeriodToRecalculate.StartDateTime.Date.Add(
					TimeHelper.FitToDefaultResolution(datePeriodToRecalculate.StartDateTime.TimeOfDay,
													  (int) defaultResolution.TotalMinutes));
			var completeIntervalPeriod = new DateTimePeriod(currentStart, currentStart.Add(defaultResolution));

			while (completeIntervalPeriod.StartDateTime < datePeriodToRecalculate.EndDateTime)
			{
				resetSkillStaffPeriodsBeforeCalculation(skills, completeIntervalPeriod);
				optimizeActivityPeriod(currentActivity, completeIntervalPeriod, resourceCalculationData);
				completeIntervalPeriod = completeIntervalPeriod.MovePeriod(defaultResolution);
			}
		}

		private void resetSkillStaffPeriodsBeforeCalculation(IList<ISkill> skills, DateTimePeriod completeIntervalPeriod)
		{
			foreach (ISkill skill in skills)
			{
				IResourceCalculationPeriodDictionary skillStaffPeriodDic;
				if (_skillStaffPeriods.TryGetValue(skill, out skillStaffPeriodDic))
				{
					IResourceCalculationPeriod skillStaffPeriod;
					var completeIntervalPeriodAdjusted = _scheduleResourcePeriodFetcher.Fetch(completeIntervalPeriod, skill);
					if (skillStaffPeriodDic.TryGetValue(completeIntervalPeriodAdjusted, out skillStaffPeriod))
					{
						skillStaffPeriod.SetCalculatedResource65(0);
						skillStaffPeriod.SetCalculatedLoggedOn(0);
					}
				}
			}
		}

		private void optimizeActivityPeriod(IActivity currentActivity, DateTimePeriod completeIntervalPeriod, ResourceCalculationData resourceCalculationData)
		{
			IDividedActivityData dividedActivityData = _activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService,
																					   currentActivity, _relevantProjections,
																					   completeIntervalPeriod);

			IEnumerable<ISkill> relevantSkills = dividedActivityData.TargetDemands.Keys;
			IDictionary<ISkill, IResourceCalculationPeriod> relevantSkillStaffPeriods =
				getRelevantSkillStaffPeriod(relevantSkills, completeIntervalPeriod);

			if (relevantSkillStaffPeriods.Count > 0 && dividedActivityData.PersonResources.Count > 0 && dividedActivityData.TargetDemands.Count > 0)
			{
				var furnessDataConverter = new FurnessDataConverter(dividedActivityData);
				IFurnessData furnessData = furnessDataConverter.ConvertDividedActivityToFurnessData();

				IFurnessEvaluator furnessEvaluator = new FurnessEvaluator(furnessData);
				furnessEvaluator.Evaluate(_quotient, _maximumIteration, Calculation.Variances.StandardDeviation);
				IDividedActivityData optimizedActivityData = furnessDataConverter.ConvertFurnessDataBackToActivity();

				if (resourceCalculationData?.SkillCombinationHolder != null)
				{
					optimizedActivityData.RelativePersonResources
						.ForEach(resources =>
								 {
									 var allSkills = resources.Key.Second;
									 resourceCalculationData.SkillCombinationHolder
										 .Add(new SkillCombinationResource
											  {
												  StartDateTime = completeIntervalPeriod.StartDateTime,
												  EndDateTime = completeIntervalPeriod.EndDateTime,
												  SkillCombination = allSkills.ToArray(),
												  Resource = resources.Value
											  });
								 });
				}
				setFurnessResultsToSkillStaffPeriods(completeIntervalPeriod, relevantSkillStaffPeriods, optimizedActivityData);
			}
		}

		private static void setFurnessResultsToSkillStaffPeriods(DateTimePeriod completeIntervalPeriod, IDictionary<ISkill, IResourceCalculationPeriod> relevantSkillStaffPeriods, IDividedActivityData optimizedActivityData)
		{
			foreach (var skillPair in relevantSkillStaffPeriods)
			{
				IResourceCalculationPeriod staffPeriod = skillPair.Value;
				double resource;
				optimizedActivityData.WeightedRelativePersonSkillResourcesSum.TryGetValue(skillPair.Key, out resource);
				double calculatedresource = resource/completeIntervalPeriod.ElapsedTime().TotalMinutes;
				staffPeriod.SetCalculatedResource65(calculatedresource);
				double loggedOn;
				optimizedActivityData.RelativePersonSkillResourcesSum.TryGetValue(skillPair.Key, out loggedOn);
				staffPeriod.SetCalculatedLoggedOn(loggedOn);
			}
		}

		private void clearSkillStaffPeriods()
		{
			foreach (var skillStaffDic in _skillStaffPeriods.Items())
			{
				if (skillStaffDic.Key.SkillType.ForecastSource == ForecastSource.InboundTelephony ||
					skillStaffDic.Key.SkillType.ForecastSource == ForecastSource.Retail)
				{
					foreach (var skillStaffPeriod in skillStaffDic.Value.OnlyValues())
					{
						skillStaffPeriod.SetCalculatedLoggedOn(0);
						skillStaffPeriod.SetCalculatedResource65(0);
					}
				}
			}
		}

		private IDictionary<ISkill, IResourceCalculationPeriod> getRelevantSkillStaffPeriod(IEnumerable<ISkill> relevantSkills, DateTimePeriod intervalPeriod)
		{
			IDictionary<ISkill, IResourceCalculationPeriod> relevantSkillStaffPeriods = new Dictionary<ISkill, IResourceCalculationPeriod>();

			foreach (ISkill skill in relevantSkills)
			{
				IResourceCalculationPeriodDictionary skillStaffPeriodDictionary;
				if (!_skillStaffPeriods.TryGetValue(skill, out skillStaffPeriodDictionary)) continue;
				IResourceCalculationPeriod staffPeriod;
				var intervalPeriodAdjusted = _scheduleResourcePeriodFetcher.Fetch(intervalPeriod, skill);
				if (skillStaffPeriodDictionary.TryGetResolutionAdjustedValue(skill, intervalPeriodAdjusted, out staffPeriod))
				{
					staffPeriod.ResetMultiskillMinOccupancy();
					relevantSkillStaffPeriods.Add(skill, staffPeriod);
				}
			}
			return relevantSkillStaffPeriods;
		}

		private IActivity[] getDistinctActivities()
		{
			return _personSkillService.AffectedSkills.Select(s => s.Activity).Distinct().ToArray();
		}
	}
}
