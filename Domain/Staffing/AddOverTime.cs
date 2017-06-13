using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
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
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly ForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;


		public AddOverTime( IUserTimeZone userTimeZone, 
						   IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ICommandDispatcher commandDispatcher, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, 
						   ScheduleOvertimeExecuteWrapper scheduleOvertimeExecuteWrapper, ScheduledStaffingToDataSeries scheduledStaffingToDataSeries, ForecastedStaffingToDataSeries forecastedStaffingToDataSeries)
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
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
		}


		public OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			//midnight shift ??
			var allSkills = _skillRepository.LoadAll().ToList();
			var skills = allSkills.Where(x => overTimeSuggestionModel.SkillIds.Contains(x.Id.GetValueOrDefault())).ToList();
			var minResolution = skills.Min(x => x.DefaultResolution);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Min(), _userTimeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max().AddMinutes(minResolution), _userTimeZone.TimeZone()));
			var userDateOnly = new DateOnly(overTimeSuggestionModel.TimeSerie.Min());

			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, period.StartDateTime, period.EndDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _currentScenario.Current(), new PersonProvider(persons), new ScheduleDictionaryLoadOptions(false, false), persons);
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();

			var scheduleDays = persons.Select(person => scheduleDictionary[person].ScheduledDay(userDateOnly)).ToList();
			
			if (!skills.Any())
				return new OverTimeSuggestionResultModel
				{
					StaffingHasData = false,
					DataSeries = null
				};

			var overtimePreferences = new OvertimePreferences
			{
				ScheduleTag = new NullScheduleTag(),
				OvertimeType = multiplicationDefinition,
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5))
			};
			
			var wrapperModels = _scheduleOvertimeExecuteWrapper.Execute(overtimePreferences, new SchedulingProgress(), scheduleDays, period, allSkills,skills);

			var returnModel = new OverTimeSuggestionResultModel() {StaffingHasData = true, DataSeries = new StaffingDataSeries()};

			returnModel.DataSeries.ScheduledStaffing =_scheduledStaffingToDataSeries.DataSeries(
					wrapperModels.ResourceCalculationPeriods.Select(x => new SkillStaffingIntervalLightModel()
					{
						StartDateTime = x.StartDateTime,
						EndDateTime = x.EndDateTime,
						StaffingLevel = x.StaffingLevel
					}).ToList(),overTimeSuggestionModel.TimeSerie);

			returnModel.DataSeries.ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(
					wrapperModels.ResourceCalculationPeriods.Select(x => new StaffingIntervalModel()
					{
						StartTime = x.StartDateTime,
						SkillId = x.SkillId,
						Agents = x.FStaff
						
					}).ToList(), overTimeSuggestionModel.TimeSerie);
			
			returnModel.DataSeries.Time = overTimeSuggestionModel.TimeSerie;
			calculateRelativeDifference(returnModel.DataSeries);
			returnModel.OverTimeModels = wrapperModels.Models;


			return returnModel;
		}

		private void calculateRelativeDifference(StaffingDataSeries dataSeries)
		{
			dataSeries.RelativeDifference = new double?[dataSeries.ForecastedStaffing.Length];
			for (var index = 0; index < dataSeries.ForecastedStaffing.Length; index++)
			{
				if (dataSeries.ForecastedStaffing[index].HasValue)
				{
					if (dataSeries.ScheduledStaffing.Length == 0)
					{
						dataSeries.RelativeDifference[index] = -dataSeries.ForecastedStaffing[index];
						continue;
					}

					if (dataSeries.ScheduledStaffing[index].HasValue)
						dataSeries.RelativeDifference[index] = dataSeries.ScheduledStaffing[index] - dataSeries.ForecastedStaffing[index];
				}

			}
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
		OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel);
		void Apply(IList<OverTimeModel> overTimeModels);
	}

	

}
