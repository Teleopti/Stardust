using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ForecastedStaffingProvider(
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			IIntervalLengthFetcher intervalLengthFetcher,
			INow now,
			IUserTimeZone timeZone
			)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_intervalLengthFetcher = intervalLengthFetcher;
			_now = now;
			_timeZone = timeZone;
		}

		public ForecastedStaffingModel Load(Guid[] skillIdList)
		{
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new List<StaffingIntervalModel>();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			TimeSpan wantedIntervalResolution = TimeSpan.FromMinutes(minutesPerInterval);
			double forecastedWorkloadSeconds = 0;

			var skills = _skillRepository.LoadSkills(skillIdList);
			foreach (var skill in skills)
			{
				var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), new[] { skill }, scenario);
				if (skillDays.Count == 0)
					continue;


				foreach (var skillDay in skillDays)
				{
					
					var skillStaffPeriods = skillDay.SkillStaffPeriodViewCollection(wantedIntervalResolution);

					staffingIntervals.AddRange(skillStaffPeriods.Select(skillStaffPeriod => new StaffingIntervalModel
					{
						StartTime = skillStaffPeriod.Period.StartDateTime,
						Agents = skillStaffPeriod.FStaff
					}));
					var callsTodayUpUntilNow = tasksTodayUpUntilNow(skillDay.SkillStaffPeriodCollection, minutesPerInterval, skill.DefaultResolution, usersNow);
					forecastedWorkloadSeconds += callsTodayUpUntilNow * (skillDay.AverageTaskTime.TotalSeconds + skillDay.AverageAfterTaskTime.TotalSeconds);
				}
			}

			return new ForecastedStaffingModel()
			{
				StaffingIntervals = staffingIntervals,
				WorkloadSeconds = forecastedWorkloadSeconds
			};
		}

		private double tasksTodayUpUntilNow(ReadOnlyCollection<ISkillStaffPeriod> skillStaffPeriodCollection, int targetMinutesPerInterval, int skillMinutesPerInterval, DateTime usersNow)
		{
			if (targetMinutesPerInterval == skillMinutesPerInterval)
			{
				return skillStaffPeriodCollection
					.Where(s => s.Period.StartDateTime < usersNow)
					.Sum(t => t.Payload.TaskData.Tasks);
			}
			return 0;
		}
	}

	public class ForecastedStaffingModel
	{
		public IList<StaffingIntervalModel> StaffingIntervals { get; set; }
		public double WorkloadSeconds { get; set; }


	}
}
