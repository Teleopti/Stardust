using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Intraday;
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

		public UpdateStaffingLevelReadModel(
			IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			INow now, IExtractSkillStaffDataForResourceCalculation extractSkillStaffDataForResourceCalculation,
			IStardustJobFeedback feedback)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_now = now;
			_extractSkillStaffDataForResourceCalculation = extractSkillStaffDataForResourceCalculation;
			_feedback = feedback;
		}

		[TestLogTime]
		public virtual void Update(DateTimePeriod period)
		{
			var periodDateOnly = new DateOnlyPeriod(new DateOnly(period.StartDateTime), new DateOnly(period.EndDateTime));
			_feedback.SendProgress($"Starting Read Model update for period {period}");
			var timeWhenResourceCalcDataLoaded = _now.UtcDateTime();
			var resCalcData =
				_extractSkillStaffDataForResourceCalculation.ExtractResourceCalculationData(periodDateOnly);
			var models = CreateReadModel(resCalcData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, period);

			setUseShrinkage(resCalcData, period);
			_extractSkillStaffDataForResourceCalculation.DoCalculation(periodDateOnly, resCalcData);

			updateModelsAfterCalculatingWithShrinkage(models, resCalcData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
				period);

			if (models.Any())
				_scheduleForecastSkillReadModelRepository.Persist(models, timeWhenResourceCalcDataLoaded);
		}

		private static void updateModelsAfterCalculatingWithShrinkage(IList<SkillStaffingInterval> models,
			ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodDictionary, DateTimePeriod period)
		{
			if (skillSkillStaffPeriodDictionary.Keys.Count > 0)
			{
				foreach (var skill in skillSkillStaffPeriodDictionary.Keys)
				{
					foreach (var skillStaffPeriod in skillSkillStaffPeriodDictionary[skill].Values)
					{
						if (!period.Contains(skillStaffPeriod.Period.StartDateTime))
							continue;
						var model =
							models.FirstOrDefault(
								w =>
									w.SkillId.Equals(skill.Id.GetValueOrDefault()) &&
									w.StartDateTime.Equals(skillStaffPeriod.Period.StartDateTime));
						if (model != null)
							model.StaffingLevelWithShrinkage = skillStaffPeriod.CalculatedResource;
					}
				}
			}
		}

		[TestLogTime]
		public virtual IList<SkillStaffingInterval> CreateReadModel(
			ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTimePeriod period)
		{
			var ret = new List<SkillStaffingInterval>();
			_feedback.SendProgress($"Will update {skillSkillStaffPeriodExtendedDictionary.Keys.Count} skills.");
			if (skillSkillStaffPeriodExtendedDictionary.Keys.Count > 0)
			{
				foreach (var skill in skillSkillStaffPeriodExtendedDictionary.Keys)
				{
					foreach (var skillStaffPeriod in skillSkillStaffPeriodExtendedDictionary[skill].Values)
					{
						if (!period.Contains(skillStaffPeriod.Period.StartDateTime))
							continue;
						ret.Add(new SkillStaffingInterval
						{
							SkillId = skill.Id.GetValueOrDefault(),
							StartDateTime = skillStaffPeriod.Period.StartDateTime,
							EndDateTime = skillStaffPeriod.Period.EndDateTime,
							Forecast = skillStaffPeriod.FStaff,
							StaffingLevel = skillStaffPeriod.CalculatedResource,
							ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage
						});
					}
					_feedback.SendProgress($"Updated {skill}.");
				}
			}
			return ret;
		}

		private static void setUseShrinkage(IResourceCalculationData resourceCalculationData, DateTimePeriod period)
		{
			var periods = resourceCalculationData.SkillStaffPeriodHolder.SkillStaffPeriodList(resourceCalculationData.Skills,
				period);
			foreach (var skillStaffPeriod in periods)
			{
				skillStaffPeriod.Payload.UseShrinkage = true;
			}
		}
	}

	public interface IUpdateStaffingLevelReadModel
	{
		void Update(DateTimePeriod period);

		IList<SkillStaffingInterval> CreateReadModel(
			ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, DateTimePeriod period);
	}
}