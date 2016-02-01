using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ISwapMainShiftForTwoPersonsCommandHandler
	{
		IEnumerable<FailActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command);
	}

	public class SwapMainShiftForTwoPersonsCommandHandler : ISwapMainShiftForTwoPersonsCommandHandler
	{
		private readonly ICommonNameDescriptionSetting _commonNameDescriptionSetting;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISwapAndModifyServiceNew _swapAndModifyServiceNew;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		

		public SwapMainShiftForTwoPersonsCommandHandler(ICommonNameDescriptionSetting commonNameDescriptionSetting,
			IPersonRepository personRepository,
			IScheduleRepository scheduleRepository,
			IScenarioRepository scenarioRepository,
			ISwapAndModifyServiceNew swapAndModifyServiceNew,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService, 
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_commonNameDescriptionSetting = commonNameDescriptionSetting;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
			_swapAndModifyServiceNew = swapAndModifyServiceNew;
			_differenceService = differenceService;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public IEnumerable<FailActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var personIds = new[] {command.PersonIdFrom, command.PersonIdTo};
			var people = _personRepository.FindPeople(personIds).ToArray();
			var scheduleDate = command.ScheduleDate;
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var scheduleDictionary = getScheduleDictionary(defaultScenario, scheduleDate, people);
			var errorResponses = swapShifts(scheduleDateOnly, people, scheduleDictionary, command.TrackedCommandInfo);

			if (errorResponses.Length > 0)
			{
				return parseErrorResponses(errorResponses);
			}

			people.ForEach( person =>
				   saveScheduleDictionaryChanges(scheduleDictionary, person));

			return new List<FailActionResult>();
		}

		private IScheduleDictionary getScheduleDictionary(IScenario scenario, DateTime date, IPerson[] people)
		{
			var period = new DateTimePeriod(date.ToUniversalTime(), date.AddDays(1).ToUniversalTime());
			var options = new ScheduleDictionaryLoadOptions(true, true);
			var schedulePeriod = new ScheduleDateTimePeriod(period);
			var personProvider = new PersonProvider(people);
			
			return _scheduleRepository.FindSchedulesForPersons(schedulePeriod, scenario, personProvider, options, people);
		}

		private IBusinessRuleResponse[] swapShifts(DateOnly date, IPerson[] people, IScheduleDictionary scheduleDictionary, TrackedCommandInfo trackedCommandInfo)
		{
			var dates = new List<DateOnly> { date};
			var lockDates = new List<DateOnly>();
			var businessRules = NewBusinessRuleCollection.Minimum();
			var scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);

			return _swapAndModifyServiceNew.Swap(people[0], people[1], dates, lockDates, scheduleDictionary, businessRules, scheduleTagSetter, trackedCommandInfo)
										   .Where(r => r.Error)
										   .ToArray();
		}

		private void saveScheduleDictionaryChanges(IScheduleDictionary scheduleDictionary, IPerson person)
		{
			var range = scheduleDictionary[person];
			var diff = range.DifferenceSinceSnapshot(_differenceService);
			_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
		}

		private IEnumerable<FailActionResult> parseErrorResponses(IBusinessRuleResponse[] errorResponses)
		{
			return errorResponses.Select(r => new FailActionResult
			{
				PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(r.Person),
				Message = new List<string> {r.Message}
			});
		}
	}
}
