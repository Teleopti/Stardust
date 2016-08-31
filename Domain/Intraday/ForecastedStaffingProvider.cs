using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public ForecastedStaffingProvider(
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher
			)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IList<StaffingIntervalModel> Load(Guid[] skillIdList, DateOnly usersToday)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new List<StaffingIntervalModel>();
			foreach (var skillId in skillIdList)
			{
				var skill = _skillRepository.Get(skillId);
				var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), new[] { skill }, scenario);
				if (skillDays.Count == 0)
					continue;

				foreach (var skillDay in skillDays)
				{
					var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));

					staffingIntervals.AddRange(skillStaffPeriods.Select(skillStaffPeriod => new StaffingIntervalModel
					{
						StartTime = skillStaffPeriod.Period.StartDateTime,
						Agents = skillStaffPeriod.FStaff
					}));
				}
			}

			return staffingIntervals;
		} 
	}
}
