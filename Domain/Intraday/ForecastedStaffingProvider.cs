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
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public ForecastedStaffingProvider(
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			INow now,
			IUserTimeZone timeZone
			)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_now = now;
			_timeZone = timeZone;
		}

		public ForecastedStaffingModel Load(Guid[] skillIdList, DateTime? latestStatisticsTime, int minutesPerInterval)
		{
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
					var callsTodayUpUntilNow = tasksTodayUpUntilNow(skillDay.SkillStaffPeriodCollection, minutesPerInterval, skill.DefaultResolution, latestStatisticsTime);
					forecastedWorkloadSeconds += callsTodayUpUntilNow * (skillDay.AverageTaskTime.TotalSeconds + skillDay.AverageAfterTaskTime.TotalSeconds);
				}
			}

			return new ForecastedStaffingModel()
			{
				StaffingIntervals = staffingIntervals,
				WorkloadSeconds = forecastedWorkloadSeconds
			};
		}

		private double tasksTodayUpUntilNow(
			ReadOnlyCollection<ISkillStaffPeriod> skillStaffPeriodCollection,
			int targetMinutesPerInterval,
			int skillMinutesPerInterval,
			DateTime? latestStatisticsTime
			)
		{
			if (!latestStatisticsTime.HasValue)
				return 0;

			if (targetMinutesPerInterval > skillMinutesPerInterval)
			{
				return 0;
			}

			if (targetMinutesPerInterval < skillMinutesPerInterval)
			{
				var splitTasksCollection = new List<myTaskPeriod>();
				var splitFactor = targetMinutesPerInterval / skillMinutesPerInterval;

				foreach (var staffPeriod in skillStaffPeriodCollection)
				{
					var tasksSplit = staffPeriod.Payload.TaskData.Tasks / splitFactor;
					for (int i = 0; i < splitFactor - 1; i++)
					{
						splitTasksCollection.Add(new myTaskPeriod
						{
							StartTime = staffPeriod.Period.StartDateTime.AddMinutes(targetMinutesPerInterval * i),
							Tasks = tasksSplit
						});
					}
				}
			}

			return skillStaffPeriodCollection
					.Where(s => s.Period.StartDateTime <= latestStatisticsTime)
					.Sum(t => t.Payload.TaskData.Tasks);
		}

		private class myTaskPeriod
		{
			public DateTime StartTime { get; set; }
			public double Tasks { get; set; }
		}
	}

	public class ForecastedStaffingModel
	{
		public IList<StaffingIntervalModel> StaffingIntervals { get; set; }
		public double WorkloadSeconds { get; set; }


	}
}
