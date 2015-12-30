using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ISwapShiftHandler
	{
		IEnumerable<FailActionResult> SwapShifts(Guid personIdFrom, Guid personIdTo, DateTime scheduleDate);
	}

	public class SwapShiftHandler : ISwapShiftHandler
	{
		private readonly ICommonNameDescriptionSetting _commonNameDescriptionSetting;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISwapAndModifyServiceNew _swapAndModifyServiceNew;
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;

		public SwapShiftHandler(ICommonNameDescriptionSetting commonNameDescriptionSetting,
			IPersonRepository personRepository,
			IScheduleRepository scheduleRepository,
			IScenarioRepository scenarioRepository,
			ISwapAndModifyServiceNew swapAndModifyServiceNew,
			IScheduleDictionaryPersister scheduleDictionaryPersister)
		{
			_commonNameDescriptionSetting = commonNameDescriptionSetting;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
			_swapAndModifyServiceNew = swapAndModifyServiceNew;
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
		}

		public IEnumerable<FailActionResult> SwapShifts(Guid personIdFrom, Guid personIdTo, DateTime scheduleDate)
		{
			var personIds = new[] {personIdFrom, personIdTo};
			var peoples = _personRepository.FindPeople(personIds).ToArray();
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var period = new DateTimePeriod(scheduleDate.ToUniversalTime(), scheduleDate.AddDays(1).ToUniversalTime());


			var schedulePeriod = new ScheduleDateTimePeriod(period);
			var personProvider = new PersonProvider(_personRepository);
			var options = new ScheduleDictionaryLoadOptions(true, true);

			var scheduleDictionary =
				_scheduleRepository.FindSchedulesForPersons(schedulePeriod, defaultScenario, personProvider, options, peoples);

			var dates = new List<DateOnly> {scheduleDateOnly};
			var lockDates = new List<DateOnly>();
			var businessRules = NewBusinessRuleCollection.Minimum();
			var scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);
			var errorResponses =
				_swapAndModifyServiceNew.Swap(peoples[0], peoples[1], dates, lockDates, scheduleDictionary,
					businessRules, scheduleTagSetter).Where(r => r.Error).ToArray();

			if (errorResponses.Length > 0)
			{
				return parseErrorResponses(errorResponses);
			}

			var conflicts = _scheduleDictionaryPersister.Persist(scheduleDictionary).ToArray();
			return parseConflicts(conflicts, peoples);
		}

		private IEnumerable<FailActionResult> parseErrorResponses(IEnumerable<IBusinessRuleResponse> errorResponses)
		{
			return errorResponses.Select(r => new FailActionResult
			{
				PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(r.Person),
				Message = new List<string> {r.Message}
			});
		}

		private IEnumerable<FailActionResult> parseConflicts(IEnumerable<PersistConflict> conflicts,
			IEnumerable<IPerson> peoples)
		{
			var conflictList = conflicts.ToList();
			var peopleList = peoples.ToList();

			var failResults = new List<FailActionResult>();
			if (!conflictList.Any())
			{
				return failResults;
			}

			foreach (var c in conflictList)
			{
				var peopleId = c.InvolvedId();
				var people = peopleList.SingleOrDefault(p => p.Id == peopleId);
				if (people != null)
				{
					failResults.Add(new FailActionResult
					{
						PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(people),
						Message = new List<string> {Resources.ScheduleHasBeenChanged}
					});
				}
			}

			return failResults;
		}
	}
}