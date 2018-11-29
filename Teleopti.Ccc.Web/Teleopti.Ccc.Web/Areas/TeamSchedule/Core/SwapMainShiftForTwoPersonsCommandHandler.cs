using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


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
		private readonly ISwapServiceNew _swapServiceNew;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IUserTimeZone _timeZone;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IPermissionProvider _permissionProvider;

		public SwapMainShiftForTwoPersonsCommandHandler(IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			ISwapServiceNew swapServiceNew,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IPermissionProvider permissionProvider,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IUserTimeZone timeZone)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_swapServiceNew = swapServiceNew;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_permissionProvider = permissionProvider;
			_differenceService = differenceService;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_timeZone = timeZone;
		}

		public IEnumerable<ActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command)
		{
			var result = new List<ActionResult>();
			if (command.PersonIdFrom == Guid.Empty
				|| command.PersonIdTo == Guid.Empty
				|| command.PersonIdFrom == command.PersonIdTo)
			{
				result.Add(new ActionResult
				{
					ErrorMessages = new List<string> { Resources.InvalidInput }
				});
				return result;
			}

			var personIds = new[] { command.PersonIdFrom, command.PersonIdTo };
			var people = _personRepository.FindPeople(personIds).ToArray();
			var scheduleDateOnly = new DateOnly(command.ScheduleDate);

			var personFrom = people[0];
			var personTo = people[1];

			var permissionErrors = checkPermission(scheduleDateOnly, personFrom, personTo);
			if (permissionErrors.Any())
			{
				result.AddRange(permissionErrors);
				return result;
			}


			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var scheduleDictionary = getScheduleDictionary(defaultScenario, scheduleDateOnly, people);
			var errorResponses = swapShifts(scheduleDateOnly, personFrom, personTo, scheduleDictionary, command.TrackedCommandInfo);

			if (errorResponses.Length > 0)
			{
				return parseErrorResponses(errorResponses);
			}

			people.ForEach(person =>
				  saveScheduleDictionaryChanges(scheduleDictionary, person));

			return result;
		}

		private IList<ActionResult> checkPermission(DateOnly scheduleDateOnly, IPerson personFrom, IPerson personTo)
		{
			var result = new List<ActionResult>();
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.SwapShifts, scheduleDateOnly, personFrom))
			{
				result.Add(new ActionResult(personFrom.Id.GetValueOrDefault())
				{
					ErrorMessages = new List<string> { Resources.NoPermissionSwapShifts }
				});
			}
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.SwapShifts, scheduleDateOnly, personTo))
			{
				result.Add(new ActionResult(personTo.Id.GetValueOrDefault())
				{
					ErrorMessages = new List<string> { Resources.NoPermissionSwapShifts }
				});
			}
			return result;
		}

		private IScheduleDictionary getScheduleDictionary(IScenario scenario, DateOnly date, IPerson[] people)
		{
			var period = date.ToDateTimePeriod(_timeZone.TimeZone());
			var options = new ScheduleDictionaryLoadOptions(false, false);

			return _scheduleStorage.FindSchedulesForPersons(scenario, people, options, period, people, false);
		}

		private IBusinessRuleResponse[] swapShifts(DateOnly date, IPerson personFrom, IPerson personTo, IScheduleDictionary scheduleDictionary, TrackedCommandInfo trackedCommandInfo)
		{
			var businessRules = NewBusinessRuleCollection.Minimum();
			var scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);

			var part1 = scheduleDictionary[personFrom].ScheduledDay(date);
			var part2 = scheduleDictionary[personTo].ScheduledDay(date);

			IList<IScheduleDay> selectedSchedules = new List<IScheduleDay> { part1, part2 };

			var modifiedParts = _swapServiceNew.Swap(scheduleDictionary, selectedSchedules, trackedCommandInfo);

			var ass1 = part1.PersonAssignment();
			ass1?.CheckRestrictions();

			var ass2 = part1.PersonAssignment();
			ass2?.CheckRestrictions();

			var responses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, modifiedParts, businessRules, _scheduleDayChangeCallback, scheduleTagSetter);

			return responses.Where(r => !r.Overridden).ToArray();
		}

		private void saveScheduleDictionaryChanges(IScheduleDictionary scheduleDictionary, IPerson person)
		{
			var range = scheduleDictionary[person];
			var diff = range.DifferenceSinceSnapshot(_differenceService);
			_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
		}

		private IEnumerable<ActionResult> parseErrorResponses(IBusinessRuleResponse[] errorResponses)
		{
			return errorResponses.Select(r => new ActionResult(r.Person.Id.GetValueOrDefault())
			{
				ErrorMessages = new List<string> { r.Message }
			});
		}
	}

}
