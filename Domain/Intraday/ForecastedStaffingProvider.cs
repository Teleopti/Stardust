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
			//var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()));
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var date = DateOnly.Today;
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var staffingIntervals = new Dictionary<DateTime, double>();
			foreach (var skillId in skillIdList)
			{
				var skill = _skillRepository.Get(skillId);
				var skillDay = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(date, date), new[] { skill }, scenario).First();
				var skillStaffPeriods = (skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval)));
				foreach (var skillStaffPeriod in skillStaffPeriods)
				{
					if (staffingIntervals.ContainsKey(skillStaffPeriod.Period.StartDateTime))
						staffingIntervals[skillStaffPeriod.Period.StartDateTime] += skillStaffPeriod.FStaff;
					else
						staffingIntervals.Add(skillStaffPeriod.Period.StartDateTime, skillStaffPeriod.FStaff);
				}
			}

			return new IntradayStaffingViewModel()
			{
				DataSeries = new StaffingDataSeries()
				{
					Time = staffingIntervals.Select(t => t.Key).ToArray(),
					ForecastedStaffing = staffingIntervals.Select(t => t.Value).ToArray()
				}
			};
		}
	}
}