using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTime
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IPersonForOvertimeProvider _personForOvertimeProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ScheduleOvertimeExecuteWrapper _scheduleOvertimeExecuteWrapper;
		private readonly INow _now;
		


		public AddOverTime( IUserTimeZone userTimeZone, 
						   ICommandDispatcher commandDispatcher, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, 
						   ScheduleOvertimeExecuteWrapper scheduleOvertimeExecuteWrapper, INow now)
		{
			_userTimeZone = userTimeZone;
			_commandDispatcher = commandDispatcher;
			_personForOvertimeProvider = personForOvertimeProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_scheduleOvertimeExecuteWrapper = scheduleOvertimeExecuteWrapper;
			_now = now;
		}


		public OvertimeWrapperModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			//midnight shift ??
			var allSkills = _skillRepository.LoadAll().ToList();
			var skills = allSkills.Where(x => overTimeSuggestionModel.SkillIds.Contains(x.Id.GetValueOrDefault())).ToList();
			if (!skills.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var minResolution = skills.Min(x => x.DefaultResolution);
			if (!overTimeSuggestionModel.TimeSerie.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var userDateOnly = new DateOnly(overTimeSuggestionModel.TimeSerie.Min());
			
			var showStartime = TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Min(), _userTimeZone.TimeZone());
			var showEndtime = TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max().AddMinutes(minResolution), _userTimeZone.TimeZone());
			var showPeriod = new DateTimePeriod(showStartime, showEndtime);

			var overtimestartTime = TimeZoneHelper.ConvertToUtc(userDateOnly.Date.AddTicks(overTimeSuggestionModel.OvertimePreferences.SelectedSpecificTimePeriod.StartTime.Ticks), _userTimeZone.TimeZone());
			var overtimeEndTime = TimeZoneHelper.ConvertToUtc(userDateOnly.Date.AddTicks(overTimeSuggestionModel.OvertimePreferences.SelectedSpecificTimePeriod.EndTime.Ticks), _userTimeZone.TimeZone());

			if (overtimestartTime < _now.UtcDateTime().AddMinutes(15)) overtimestartTime = _now.UtcDateTime().AddMinutes(15);
			var overtimePeriod = new DateTimePeriod(overtimestartTime, overtimeEndTime);
			
			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, overtimestartTime, overtimeEndTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));

			if(!persons.Any()) return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var weeks = new List<DateTimePeriod>();
			foreach (var person in persons)
			{
				var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(userDateOnly, person.FirstDayOfWeek);
				weeks.Add(new DateTimePeriod(firstDateInPeriodLocal.ToDateTimePeriod(_userTimeZone.TimeZone()).StartDateTime, firstDateInPeriodLocal.AddDays(6).ToDateTimePeriod(_userTimeZone.TimeZone()).EndDateTime));
			}

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(weeks.Min(x => x.StartDateTime), weeks.Max(x => x.EndDateTime))), _currentScenario.Current(), new PersonProvider(persons), 
							new ScheduleDictionaryLoadOptions(false, false), persons);
			
			return _scheduleOvertimeExecuteWrapper.Execute(overTimeSuggestionModel.OvertimePreferences, new SchedulingProgress(), scheduleDictionary, persons, new DateOnlyPeriod(userDateOnly.AddDays(-1), userDateOnly.AddDays(1)), overtimePeriod, allSkills,skills, showPeriod);
		}

		public void Apply(IList<OverTimeModel> overTimeModels, Guid multiplicationDefinitionId)
		{
			
			foreach (var overTimeModel in overTimeModels)
			{
				_commandDispatcher.Execute(new AddOvertimeActivityCommand
										   {
											   ActivityId = overTimeModel.ActivityId,
											   Date = new DateOnly(overTimeModel.StartDateTime),
											   MultiplicatorDefinitionSetId = multiplicationDefinitionId,
											   Period = new DateTimePeriod(overTimeModel.StartDateTime.Utc(), overTimeModel.EndDateTime.Utc()),
											   PersonId = overTimeModel.PersonId
										   });
				
			}
		}
		
	}

}
