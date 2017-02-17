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
		private readonly CalculateOvertimeSuggestionProvider _calculateOvertimeSuggestionProvider;

		public AddOverTime(TimeSeriesProvider timeSeriesProvider,
						   ScheduledStaffingProvider scheduledStaffingProvider,
						   IIntervalLengthFetcher intervalLengthFetcher,
						   ISkillRepository skillRepository,
						   ForecastedStaffingProvider forecastedStaffingProvider,
						   ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository,
						   INow now, IUserTimeZone timeZone, CalculateOvertimeSuggestionProvider calculateOvertimeSuggestionProvider)
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
			_calculateOvertimeSuggestionProvider = calculateOvertimeSuggestionProvider;
		}

		public OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersTomorrow = new DateOnly(usersNow.AddHours(24));
			var userstomorrowUtc = TimeZoneHelper.ConvertToUtc(usersTomorrow.Date, _timeZone.TimeZone());
			var overTimeStaffingSuggestion = _calculateOvertimeSuggestionProvider.GetOvertimeSuggestions(overTimeSuggestionModel.SkillIds, _now.UtcDateTime(), userstomorrowUtc);
			var overTimescheduledStaffingPerSkill = overTimeStaffingSuggestion.SkillStaffingIntervals.Select(x => new SkillStaffingIntervalLightModel
																			 {
																				 Id = x.SkillId,
																				 StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _timeZone.TimeZone()),
																				 EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _timeZone.TimeZone()),
																				 StaffingLevel = x.StaffingLevel
																			 }).ToList();
			return new OverTimeSuggestionResultModel
			{
				SuggestedStaffingWithOverTime = _scheduledStaffingProvider.DataSeries(overTimescheduledStaffingPerSkill, overTimeSuggestionModel.TimeSerie),
				OverTimeModels = overTimeStaffingSuggestion.OverTimeModels
			};
		}
	}

	public interface IAddOverTime
	{
		OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel skillIds);
	}

	public class OverTimeStaffingSuggestionModel
	{
		public IList<SkillStaffingInterval> SkillStaffingIntervals { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}


	public class OverTimeSuggestionResultModel
	{
		public double?[] SuggestedStaffingWithOverTime { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}

	public class OverTimeSuggestionModel
	{
		public IList<Guid> SkillIds { get; set; }
		public DateTime[] TimeSerie { get; set; }
	}

	public class OverTimeModel
	{
		public Guid ActivityId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

}
