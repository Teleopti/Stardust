using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftLayerCommandHandler : IHandleCommand<MoveShiftLayerCommand>
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly ICurrentScenario _currentScenario;

		public MoveShiftLayerCommandHandler(IScheduleStorage scheduleStorage, IScheduleDifferenceSaver scheduleDifferenceSaver, IProxyForId<IPerson> personForId, ICurrentScenario currentScenario)
		{
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_personForId = personForId;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveShiftLayerCommand command)
		{
			var person = _personForId.Load(command.AgentId);
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

			var shiftLayer = personAss.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages = new List<string> { Resources.NoShiftsFound };
				return;
			}
			personAss.MoveActivityAndKeepOriginalPriority(shiftLayer, command.NewStartTimeInUtc, command.TrackedCommandInfo);
			dic.Modify(scheduledDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}
}