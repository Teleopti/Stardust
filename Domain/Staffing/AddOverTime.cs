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
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;

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
		private readonly IActivityRepository _activityRepository;
		private readonly ScheduleOvertimeExecuteWrapper _scheduleOvertimeExecuteWrapper;
		private readonly INow _now;
		


		public AddOverTime( IUserTimeZone userTimeZone, 
						   ICommandDispatcher commandDispatcher, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, 
						   ScheduleOvertimeExecuteWrapper scheduleOvertimeExecuteWrapper, INow now, IActivityRepository activityRepository)
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
			_activityRepository = activityRepository;
		}


		public OvertimeWrapperModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			//midnight shift ??
			_activityRepository.LoadAll();
			var allSkills = _skillRepository.LoadAll().ToList();
			var skills = allSkills.Where(x => overTimeSuggestionModel.SkillIds.Contains(x.Id.GetValueOrDefault())).ToList();
			if (!skills.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var minResolution = skills.Min(x => x.DefaultResolution);
			if (!overTimeSuggestionModel.TimeSerie.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var localDateTime = overTimeSuggestionModel.TimeSerie.Min();
			var userDateOnly = new DateOnly(localDateTime);

			var userTimeZone = _userTimeZone.TimeZone();
			var showStartime = TimeZoneHelper.ConvertToUtc(localDateTime, userTimeZone);
			var showEndtime = TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max().AddMinutes(minResolution), userTimeZone);
			var showPeriod = new DateTimePeriod(showStartime, showEndtime);

			var overtimestartTime = TimeZoneHelper.ConvertToUtc(userDateOnly.Date.Add(overTimeSuggestionModel.OvertimePreferences.SelectedSpecificTimePeriod.StartTime), userTimeZone);
			var overtimeEndTime = TimeZoneHelper.ConvertToUtc(userDateOnly.Date.Add(overTimeSuggestionModel.OvertimePreferences.SelectedSpecificTimePeriod.EndTime), userTimeZone);

			var nowUtc = _now.UtcDateTime();
			if (overtimestartTime < nowUtc.AddMinutes(15)) overtimestartTime = nowUtc.AddMinutes(15);

			if(overtimestartTime > overtimeEndTime) return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());
			var overtimePeriod = new DateTimePeriod(overtimestartTime, overtimeEndTime);

			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, overtimestartTime,
				overtimeEndTime, overTimeSuggestionModel.OvertimePreferences.OvertimeType.Id.GetValueOrDefault(),
				overTimeSuggestionModel.NumberOfPersonsToTry);

			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));

			if(!persons.Any()) return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var weeks = new HashSet<DateTimePeriod>();
			foreach (var person in persons)
			{
				var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(userDateOnly, person.FirstDayOfWeek);
				var period = new DateOnlyPeriod(firstDateInPeriodLocal,firstDateInPeriodLocal.AddDays(6)).ToDateTimePeriod(userTimeZone);
				weeks.Add(period);
			}

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(_currentScenario.Current(), persons, 
							new ScheduleDictionaryLoadOptions(false, false), new DateTimePeriod(weeks.Min(x => x.StartDateTime), weeks.Max(x => x.EndDateTime)), persons, false);
			
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
											   Person = _personRepository.Get(overTimeModel.PersonId)
										   });
				
			}
		}
	}
}