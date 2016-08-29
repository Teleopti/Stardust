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
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public ForecastedStaffingProvider(
			ISkillRepository skillRepository,
			ISkillDayRepository skillDayRepository,
			IScenarioRepository scenarioRepository,
			INow now,
			IUserTimeZone timeZone,
			IIntervalLengthFetcher intervalLengthFetcher)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_now = now;
			_timeZone = timeZone;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IntradayStaffingViewModel Load(Guid[] skillIdList)
		{
			var usersToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new Dictionary<DateTime, double>();
			foreach (var skillId in skillIdList)
			{
				var skill = _skillRepository.Get(skillId);
				var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), new[] { skill }, scenario);
				if (skillDays.Count == 0)
					continue;
				
				foreach (var skillDay in skillDays)
				{
					var skillStaffPeriods = (skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval)));

					foreach (var skillStaffPeriod in skillStaffPeriods)
					{
						var start = TimeZoneHelper.ConvertFromUtc(skillStaffPeriod.Period.StartDateTime, _timeZone.TimeZone());
						if (staffingIntervals.ContainsKey(start))
							staffingIntervals[start] += skillStaffPeriod.FStaff;
						else
							staffingIntervals.Add(start, skillStaffPeriod.FStaff);
					}
				}
			}

			var staffingForUsersToday = staffingIntervals
												.Where(t => t.Key >= usersToday.Date && t.Key < usersToday.Date.AddDays(1))
												.ToArray();
			return new IntradayStaffingViewModel()
			{
				DataSeries = new StaffingDataSeries()
				{
					Time = staffingForUsersToday
								.Select(t => t.Key)
								.ToArray(),
					ForecastedStaffing = staffingForUsersToday
								.Select(t => t.Value)
								.ToArray()
				}
			};
		}
	}
}