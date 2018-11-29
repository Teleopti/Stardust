using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeRepositories;

using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class SkillIntradayStaffingFactory
	{
		private readonly FakeSkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _scenario;
		private readonly FakeSkillCombinationResourceRepository _combinationRepository;

		public SkillIntradayStaffingFactory(FakeSkillDayRepository skillDayRepository, ICurrentScenario scenario,
			FakeSkillCombinationResourceRepository combinationRepository)
		{
			_skillDayRepository = skillDayRepository;
			_scenario = scenario;
			_combinationRepository = combinationRepository;
		}

		public void SetupIntradayStaffingForSkill(ISkill skill, DateOnly date,
			IEnumerable<StaffingPeriodData> staffingPeriodDatas, TimeZoneInfo timezone, ServiceAgreement? serviceAgreement = null)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();

			foreach (var staffingPeriodData in staffingPeriodDatas)
			{
				skillCombinationResources.AddRange(
					createSkillCombinationResources(
						skill, 
						staffingPeriodData.Period,
						staffingPeriodData.ScheduledStaffing));
				skillForecastedStaffings.AddRange(
					createSkillForecastedStaffings(
						skill, 
						staffingPeriodData.Period,
						staffingPeriodData.ForecastedStaffing, 
						timezone));
			}

			setupIntradayStaffingForSkill(skill, date, skillCombinationResources, skillForecastedStaffings, serviceAgreement);
		}

		private void setupIntradayStaffingForSkill(ISkill skill, DateOnly date,
			IEnumerable<SkillCombinationResource> skillCombinationResources,
			IEnumerable<Tuple<TimePeriod, double>> skillForecastedStaffings,
			ServiceAgreement? serviceAgreement = null)
		{
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				_combinationRepository.AddSkillCombinationResource(new DateTime(),
					new[]
					{
						skillCombinationResource
					});
			}

			var skillDay = skill.CreateSkillDayWithDemandOnInterval(_scenario.Current(),
				date, 0, serviceAgreement ?? ServiceAgreement.DefaultValues(),
				skillForecastedStaffings.ToArray()).WithId();
			skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });
			_skillDayRepository.Has(skillDay);
		}

		private List<SkillCombinationResource> createSkillCombinationResources(ISkill skill, DateTimePeriod dateTimePeriod, double scheduledStaffing)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var intervals = dateTimePeriod.Intervals(TimeSpan.FromMinutes(skill.DefaultResolution));
			for (var i = 0; i < intervals.Count; i++)
			{
				skillCombinationResources.Add(
					new SkillCombinationResource
					{
						StartDateTime = intervals[i].StartDateTime,
						EndDateTime = intervals[i].EndDateTime,
						Resource = scheduledStaffing,
						SkillCombination = new[] { skill.Id.Value }
					}
				);
			}
			return skillCombinationResources;
		}

		private List<Tuple<TimePeriod, double>> createSkillForecastedStaffings(ISkill skill, DateTimePeriod dateTimePeriod, double forecastedStaffing, TimeZoneInfo timezone)
		{
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();

			for (var time = dateTimePeriod.StartDateTimeLocal(timezone);
				time < dateTimePeriod.EndDateTimeLocal(timezone);
				time = time.AddMinutes(skill.DefaultResolution))
			{
				var endTime = time.AddMinutes(skill.DefaultResolution);
				skillForecastedStaffings.Add(new Tuple<TimePeriod, double>(
					new TimePeriod(time.TimeOfDay, endTime.Day != time.Day? endTime.TimeOfDay.Add(TimeSpan.FromDays(1)):endTime.TimeOfDay),
					forecastedStaffing));
			}
			return skillForecastedStaffings;
		}
	}

	public class StaffingPeriodData
	{
		public DateTimePeriod Period;

		public double ForecastedStaffing;

		public double ScheduledStaffing;
	}
}
