using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class UpdateStaffingLevelReadModel : IUpdateStaffingLevelReadModel
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly ExtractSkillStaffingDataForResourceCalculation _extractSkillStaffingDataForResourceCalculation;
		private readonly INow _now;
		private readonly IStardustJobFeedback _feedback;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public UpdateStaffingLevelReadModel(
			IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			INow now, ExtractSkillStaffingDataForResourceCalculation extractSkillStaffingDataForResourceCalculation,
			IStardustJobFeedback feedback, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_now = now;
			_extractSkillStaffingDataForResourceCalculation = extractSkillStaffingDataForResourceCalculation;
			_feedback = feedback;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}
		
		public void Update(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			_feedback.SendProgress($"Starting ForecastSkill Read Model update for period {period}");
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			var resCalcData =
				_extractSkillStaffingDataForResourceCalculation.ExtractResourceCalculationData(periodDateOnly);

			updateFromResourceCalculationData(period, resCalcData, periodDateOnly, timeWhenResourceCalcDataLoaded);
		}

		private void updateFromResourceCalculationData(DateTimePeriod period, ResourceCalculationData resCalcData,
			DateOnlyPeriod periodDateOnly, DateTime timeWhenResourceCalcDataLoaded)
		{
			var models = createReadModel(resCalcData.SkillResourceCalculationPeriodDictionary, period);

			setUseShrinkage(resCalcData);
			_extractSkillStaffingDataForResourceCalculation.DoCalculation(periodDateOnly, resCalcData);

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
								x.StartDateTime >= period.StartDateTime &&
								x.StartDateTime < period.EndDateTime);
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
					{
						model.StaffingLevelWithShrinkage = ((ISkillStaffPeriod)periodPair.Value).CalculatedResource;
						model.ForecastWithShrinkage = ((ISkillStaffPeriod)periodPair.Value).FStaff;
					}
						
				}
			}
		}
		
		private IList<SkillStaffingInterval> createReadModel(
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
						ForecastWithShrinkage = periodPair.Value.FStaff
					});
				_feedback.SendProgress($"Updated {skill}.");
			}
			return ret;
		}

		private static void setUseShrinkage(ResourceCalculationData resourceCalculationData)
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
	}
}