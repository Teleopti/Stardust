using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveActivityCommandHandler : IHandleCommand<RemoveActivityCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public RemoveActivityCommandHandler( ICurrentScenario currentScenario, IProxyForId<IPerson> personForId, IScheduleStorage scheduleStorage, IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_currentScenario = currentScenario;
			_personForId = personForId;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(RemoveActivityCommand command)
		{

			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var period = new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc());
			var dic = _scheduleStorage.FindSchedulesForPersons(scenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), period, new[] { person }, false);
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();
			
			command.ErrorMessages = new List<string>();

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.PersonAssignmentIsNotValid);
				return;
			}

			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages.Add(Resources.NoShiftsFound);
				return;
			}

			if (personAssignment.ShiftLayers.Count() == 1)
			{
				command.ErrorMessages.Add(Resources.CannotDeleteBaseActivity);
				return;
			}
			personAssignment.RemoveActivity(shiftLayer, false, command.TrackedCommandInfo);
			dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}
}
