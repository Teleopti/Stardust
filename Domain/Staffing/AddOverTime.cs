using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTime : IAddOverTime
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IPersonForOvertimeProvider _personForOvertimeProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ScheduleOvertimeExecuteWrapper _scheduleOvertimeExecuteWrapper;
		private readonly INow _now;
		


		public AddOverTime( IUserTimeZone userTimeZone, 
						   IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ICommandDispatcher commandDispatcher, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, 
						   ScheduleOvertimeExecuteWrapper scheduleOvertimeExecuteWrapper, INow now)
		{
			_userTimeZone = userTimeZone;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
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
			var minResolution = skills.Min(x => x.DefaultResolution);
			if (!overTimeSuggestionModel.TimeSerie.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var startTime = TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Min(), _userTimeZone.TimeZone());
			var endTime = TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max().AddMinutes(minResolution), _userTimeZone.TimeZone());
			var fullPeriod = new DateTimePeriod(startTime, endTime);
			
			if (startTime < _now.UtcDateTime().AddMinutes(15)) startTime = _now.UtcDateTime().AddMinutes(15);
			var period = new DateTimePeriod(startTime, endTime);

			var userDateOnly = new DateOnly(overTimeSuggestionModel.TimeSerie.Min());
			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, period.StartDateTime, period.EndDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _currentScenario.Current(), new PersonProvider(persons), 
							new ScheduleDictionaryLoadOptions(false, false), persons);

			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();
		
			if (!skills.Any())
				return new OvertimeWrapperModel(new List<SkillStaffingInterval>(), new List<OverTimeModel>());

			var overtimePreferences = new OvertimePreferences
			{
				ScheduleTag = new NullScheduleTag(),
				OvertimeType = multiplicationDefinition,
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5))
			};
			
			return _scheduleOvertimeExecuteWrapper.Execute(overtimePreferences, new SchedulingProgress(), scheduleDictionary, persons, userDateOnly.ToDateOnlyPeriod(), period, allSkills,skills, fullPeriod);
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
				
			}
		}
		
	}

	public interface IAddOverTime
	{
		OvertimeWrapperModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel);
		void Apply(IList<OverTimeModel> overTimeModels);
	}

	

}
