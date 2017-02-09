using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTime : IAddOverTime
	{
		private readonly TimeSeriesProvider _timeSeriesProvider;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly ISkillRepository _skillRepository;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public AddOverTime(TimeSeriesProvider timeSeriesProvider,
						   ScheduledStaffingProvider scheduledStaffingProvider,
						   IIntervalLengthFetcher intervalLengthFetcher,
						   ISkillRepository skillRepository, 
						   ForecastedStaffingProvider forecastedStaffingProvider, 
						   ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository, 
						   INow now, IUserTimeZone timeZone)
		{
			_timeSeriesProvider = timeSeriesProvider;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_intervalLengthFetcher = intervalLengthFetcher;
			_skillRepository = skillRepository;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_now = now;
			_timeZone = timeZone;
		}

		public IntradayStaffingViewModel GetSuggestion(IEnumerable<Guid> skillIds)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersToday = new DateOnly(usersNow);
			var skills = _skillRepository.LoadSkills(skillIds);
			var minutesPerInterval = _intervalLengthFetcher.IntervalLength;
			var scheduledStaffingPerSkill = _scheduledStaffingProvider.StaffingPerSkill(skills.ToList(), minutesPerInterval);
			var fakeOverTimescheduledStaffingPerSkill = scheduledStaffingPerSkill.Select(interval => new SkillStaffingIntervalLightModel
																						 {
																							 StartDateTime = interval.StartDateTime,
																							 EndDateTime = interval.EndDateTime,
																							 Id = interval.Id,
																							 StaffingLevel = interval.StaffingLevel + 1
																						 }).ToList();
			var skillDays = _skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(usersToday.AddDays(-1), usersToday.AddDays(1)), skills, scenario);
			var forecastedStaffing = _forecastedStaffingProvider.StaffingPerSkill(skills.ToList(), skillDays, minutesPerInterval);
			var timeSeries = _timeSeriesProvider.DataSeries(forecastedStaffing, scheduledStaffingPerSkill, minutesPerInterval);

			return new IntradayStaffingViewModel
			{
				DataSeries = new StaffingDataSeries
				{
					Time = timeSeries,
					ScheduledStaffing = _scheduledStaffingProvider.DataSeries(fakeOverTimescheduledStaffingPerSkill, timeSeries)
				},
				StaffingHasData = timeSeries.Any()
			};
		}
	}

	public interface IAddOverTime
	{
		IntradayStaffingViewModel GetSuggestion(IEnumerable<Guid> skillIds);
	}
}
