using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftCommandHandler : IHandleCommand<MoveShiftCommand>
	{
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly ICurrentScenario _currentScenario;

		public MoveShiftCommandHandler(IProxyForId<IPerson> personForId, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_personForId = personForId;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveShiftCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var currentScenario = _currentScenario.Current();
			var currentDate = command.ScheduleDate;

			var dic = _scheduleStorage.FindSchedulesForPersons(currentScenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), currentDate.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { person }, false);
			var scheduleRange = dic[person];
			var scheduledDay = scheduleRange.ScheduledDay(currentDate);
			var personAss = scheduledDay.PersonAssignment();
			
			if (personAss == null)
			{
				command.ErrorMessages = new List<string> { Resources.PersonAssignmentIsNotValid };
				return;
			}

			personAss.MoveAllActivitiesAndKeepOriginalPriority(command.NewStartTimeInUtc, command.TrackedCommandInfo);
			dic.Modify(scheduledDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}
}