using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
		private readonly IMessagePopulatingServiceBusSender _busSender;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public SwapMainShiftForTwoPersonsCommandHandler(ICommonNameDescriptionSetting commonNameDescriptionSetting,
			IPersonRepository personRepository,
			IScheduleRepository scheduleRepository,
			IScenarioRepository scenarioRepository,
			ISwapAndModifyServiceNew swapAndModifyServiceNew,
			IMessagePopulatingServiceBusSender busSender,
			ICurrentUnitOfWork currentUnitOfWork)
		{
			_commonNameDescriptionSetting = commonNameDescriptionSetting;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
			_swapAndModifyServiceNew = swapAndModifyServiceNew;
			_busSender = busSender;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<FailActionResult> SwapShifts(SwapMainShiftForTwoPersonsCommand command)
		{
			var personIds = new[] {command.PersonIdFrom, command.PersonIdTo};
			var peoples = _personRepository.FindPeople(personIds).ToArray();

			var scheduleDate = command.ScheduleDate;
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var period = new DateTimePeriod(scheduleDate.ToUniversalTime(), scheduleDate.AddDays(1).ToUniversalTime());

			var schedulePeriod = new ScheduleDateTimePeriod(period);
			var personProvider = new PersonProvider(peoples);
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

			var uow = _currentUnitOfWork.Current();
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
				var personAssignment = scheduleRange.ScheduledDay(scheduleDateOnly).PersonAssignment(true);
				uow.Merge(personAssignment);
			}
			uow.PersistAll();

			notifyScheduleChanged(scheduleDate, scheduleDate.AddDays(1), defaultScenario.Id.Value, personIds,
				command.TrackedCommandInfo);

			return new List<FailActionResult>();
		}

		private IEnumerable<FailActionResult> parseErrorResponses(IEnumerable<IBusinessRuleResponse> errorResponses)
		{
			return errorResponses.Select(r => new FailActionResult
			{
				PersonName = _commonNameDescriptionSetting.BuildCommonNameDescription(r.Person),
				Message = new List<string> {r.Message}
			});
		}

		private void notifyScheduleChanged(DateTime startTime, DateTime endTime, Guid scenarioId, IEnumerable<Guid> personIds,
			TrackedCommandInfo trackedCommandInfo)
		{
			foreach (var personId in personIds)
			{
				var message = new ScheduleChangedEvent
				{
					StartDateTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc),
					EndDateTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc),
					ScenarioId = scenarioId,
					PersonId = personId,
					InitiatorId = trackedCommandInfo != null ? trackedCommandInfo.OperatedPersonId : Guid.Empty,
					TrackId = trackedCommandInfo != null ? trackedCommandInfo.TrackId : Guid.Empty,
					Timestamp = DateTime.UtcNow
				};

				_busSender.Send(message, true);
			}
		}
	}
}
