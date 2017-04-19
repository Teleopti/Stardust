using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTime : IAddOverTime
	{
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateOvertimeSuggestionProvider _calculateOvertimeSuggestionProvider;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public AddOverTime(ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
						   INow now, IUserTimeZone timeZone, CalculateOvertimeSuggestionProvider calculateOvertimeSuggestionProvider, 
						   IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ICommandDispatcher commandDispatcher, 
						   ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_now = now;
			_timeZone = timeZone;
			_calculateOvertimeSuggestionProvider = calculateOvertimeSuggestionProvider;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
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
				SuggestedStaffingWithOverTime = _scheduledStaffingToDataSeries.DataSeries(overTimescheduledStaffingPerSkill, overTimeSuggestionModel.TimeSerie),
				OverTimeModels = overTimeStaffingSuggestion.OverTimeModels
			};
		}

		public void Apply(IList<OverTimeModel> overTimeModels )
		{
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();
			if (multiplicationDefinition == null) return;

			foreach (var overTimeModel in overTimeModels)
			{
				_commandDispatcher.Execute(new AddOvertimeActivityCommand
										   {
											   ActivityId = overTimeModel.ActivityId,
											   Date = new DateOnly(overTimeModel.StartDateTime),
											   MultiplicatorDefinitionSetId = multiplicationDefinition.Id.GetValueOrDefault(),
											   Period = new DateTimePeriod(overTimeModel.StartDateTime.Utc(), overTimeModel.EndDateTime.Utc()),
											   PersonId = overTimeModel.PersonId
										   });

				_skillCombinationResourceRepository.PersistChanges(overTimeModel.Deltas);

			}
			
			

		}
	}

	public interface IAddOverTime
	{
		OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel);
		void Apply(IList<OverTimeModel> overTimeModels);
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
		public IList<SkillCombinationResource> Deltas { get; set; }
	}

}
