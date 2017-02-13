﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class UpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly IExtractSkillStaffDataForResourceCalculation _extractSkillStaffDataForResourceCalculation;
		private readonly INow _now;
		private readonly IStardustJobFeedback _feedback;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public UpdateStaffingLevelReadModel(
			IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			INow now, IExtractSkillStaffDataForResourceCalculation extractSkillStaffDataForResourceCalculation,
			IStardustJobFeedback feedback, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_now = now;
			_extractSkillStaffDataForResourceCalculation = extractSkillStaffDataForResourceCalculation;
			_feedback = feedback;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		[TestLog]
		public virtual void Update(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			_feedback.SendProgress($"Starting ForecastSkill Read Model update for period {period}");
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			var resCalcData =
				_extractSkillStaffDataForResourceCalculation.ExtractResourceCalculationData(periodDateOnly);

			UpdateFromResourceCalculationData(period, resCalcData, periodDateOnly, timeWhenResourceCalcDataLoaded);
		}

		public void UpdateFromResourceCalculationData(DateTimePeriod period, IResourceCalculationData resCalcData,
			DateOnlyPeriod periodDateOnly, DateTime timeWhenResourceCalcDataLoaded)
		{
			var models = CreateReadModel(resCalcData.SkillResourceCalculationPeriodDictionary, period);

			setUseShrinkage(resCalcData, period);
			_extractSkillStaffDataForResourceCalculation.DoCalculation(periodDateOnly, resCalcData);

			updateModelsAfterCalculatingWithShrinkage(models, resCalcData.SkillResourceCalculationPeriodDictionary, period);

			if (models.Any())
			{
				_scheduleForecastSkillReadModelRepository.Persist(models, timeWhenResourceCalcDataLoaded);
				if (resCalcData.SkillCombinationHolder != null)
				{
					_feedback.SendProgress("Starting SkillCombinationResource Read Model update.");
					var filteredCombinations =
						resCalcData.SkillCombinationHolder.SkillCombinationResources.Where(
							x =>
								x.StartDateTime > timeWhenResourceCalcDataLoaded.AddHours(-1) &&
								x.StartDateTime < timeWhenResourceCalcDataLoaded.AddHours(25));
					_skillCombinationResourceRepository.PersistSkillCombinationResource(timeWhenResourceCalcDataLoaded,
						filteredCombinations);
				}
			}
			_feedback.SendProgress("Starting purge ForecastSkill Read Model");
			_scheduleForecastSkillReadModelRepository.Purge();
		}

		private static void updateModelsAfterCalculatingWithShrinkage(IList<SkillStaffingInterval> models,
			ISkillResourceCalculationPeriodDictionary skillResourceCalculationPeriodDictionary, DateTimePeriod period)
		{
			if (!skillResourceCalculationPeriodDictionary.Items().Any()) return;

			foreach (var keyValuePair in skillResourceCalculationPeriodDictionary.Items())
			{
				var skill = keyValuePair.Key;
				var dic = keyValuePair.Value;
				foreach (var periodPair in dic.Items())
				{
					if (!period.Contains(periodPair.Key.StartDateTime))
						continue;
					var model =
						models.FirstOrDefault(
							w =>
								w.SkillId.Equals(skill.Id.GetValueOrDefault()) &&
								w.StartDateTime.Equals(periodPair.Key.StartDateTime));
					if (model != null)
						model.StaffingLevelWithShrinkage = ((ISkillStaffPeriod)periodPair.Value).CalculatedResource;
				}
			}
		}

		[TestLog]
		public virtual IList<SkillStaffingInterval> CreateReadModel(
			ISkillResourceCalculationPeriodDictionary skillResourceCalculationPeriodDictionary, DateTimePeriod period)
		{
			var ret = new List<SkillStaffingInterval>();
			_feedback.SendProgress($"Will update {skillResourceCalculationPeriodDictionary.Items().Count()} skills.");
			if (!skillResourceCalculationPeriodDictionary.Items().Any()) return ret;
			foreach (var keyValuePair in skillResourceCalculationPeriodDictionary.Items())
			{
				var skill = keyValuePair.Key;
				var dic = keyValuePair.Value;
				ret.AddRange(from periodPair in dic.Items()
					where period.Contains(periodPair.Key.StartDateTime)
					let skillStaffPeriod = (ISkillStaffPeriod) periodPair.Value
					select new SkillStaffingInterval
					{
						SkillId = skill.Id.GetValueOrDefault(),
						StartDateTime = periodPair.Key.StartDateTime,
						EndDateTime = periodPair.Key.EndDateTime,
						Forecast = periodPair.Value.FStaff,
						StaffingLevel = skillStaffPeriod.CalculatedResource,
						ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage
					});
				_feedback.SendProgress($"Updated {skill}.");
			}
			return ret;
		}

		private static void setUseShrinkage(IResourceCalculationData resourceCalculationData, DateTimePeriod period)
		{
			resourceCalculationData.SkillCombinationHolder?.StartRecodingValuesWithShrinkage();
			
			var items = resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items();
			foreach (var keyValuePair in items)
			{
				var dic = keyValuePair.Value;
				foreach (var resourceCalculationPeriod in dic.OnlyValues())
				{
					((ISkillStaffPeriod) resourceCalculationPeriod).Payload.UseShrinkage = true;
				}
			}

		}
	}

	public interface IUpdateStaffingLevelReadModel
	{
		void Update(DateTimePeriod period);

		void UpdateFromResourceCalculationData(DateTimePeriod period, IResourceCalculationData resCalcData,
			DateOnlyPeriod periodDateOnly, DateTime timeWhenResourceCalcDataLoaded);

		IList<SkillStaffingInterval> CreateReadModel(
			ISkillResourceCalculationPeriodDictionary skillSkillStaffPeriodExtendedDictionary, DateTimePeriod period);
	}
}