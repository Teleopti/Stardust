using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ISwapMainShiftForTwoPersonsCommandHandler
	{
		IEnumerable<ActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command);
	}

	public class SwapMainShiftForTwoPersonsCommandHandler : ISwapMainShiftForTwoPersonsCommandHandler
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISwapAndModifyServiceNew _swapAndModifyServiceNew;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IUserTimeZone _timeZone;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		
		public SwapMainShiftForTwoPersonsCommandHandler(IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			ISwapAndModifyServiceNew swapAndModifyServiceNew,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService, 
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IUserTimeZone timeZone)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_swapAndModifyServiceNew = swapAndModifyServiceNew;
			_differenceService = differenceService;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_timeZone = timeZone;
		}

		public IEnumerable<ActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var personIds = new[] {command.PersonIdFrom, command.PersonIdTo};
			var people = _personRepository.FindPeople(personIds).ToArray();
			var scheduleDateOnly = new DateOnly(command.ScheduleDate);
			var scheduleDictionary = getScheduleDictionary(defaultScenario, scheduleDateOnly, people);
			var errorResponses = swapShifts(scheduleDateOnly, people, scheduleDictionary, command.TrackedCommandInfo);

			if (errorResponses.Length > 0)
			{
				return parseErrorResponses(errorResponses);
			}

			people.ForEach( person =>
				   saveScheduleDictionaryChanges(scheduleDictionary, person));

			return new List<ActionResult>();
		}

		private IScheduleDictionary getScheduleDictionary(IScenario scenario, DateOnly date, IPerson[] people)
		{
			var period = date.ToDateTimePeriod(_timeZone.TimeZone());
			var options = new ScheduleDictionaryLoadOptions(true, true);
			var schedulePeriod = new ScheduleDateTimePeriod(period);
			var personProvider = new PersonProvider(people)
			{
				DoLoadByPerson = true
			};

			return _scheduleStorage.FindSchedulesForPersons(schedulePeriod, scenario, personProvider, options, people);
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

		private IEnumerable<ActionResult> parseErrorResponses(IBusinessRuleResponse[] errorResponses)
		{
			return errorResponses.Select(r => new ActionResult
			{
				PersonId = r.Person.Id.GetValueOrDefault(),
				ErrorMessages = new List<string> {r.Message}
			});
		}
	}

}
