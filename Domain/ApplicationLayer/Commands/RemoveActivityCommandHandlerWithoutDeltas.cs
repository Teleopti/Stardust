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

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveActivityCommandHandler : IHandleCommand<RemoveActivityCommand>
	{
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public RemoveActivityCommandHandler(
			IProxyForId<IPerson> personForId, 
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IScheduleDayProvider scheduleDayProvider)
		{
			_personForId = personForId;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_scheduleDayProvider = scheduleDayProvider;
		}

		public void Handle(RemoveActivityCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var period = new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc());
			var scheduleDic = _scheduleDayProvider.GetScheduleDictionary(command.Date, person);

			var scheduleRange = scheduleDic[person];
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

			if (shiftLayer is MainShiftLayer mainShiftLayer && mainShiftLayer.OrderIndex == 0)
			{
				command.ErrorMessages.Add(Resources.CannotDeleteBaseActivity);
				return;
			}
			
			personAssignment.RemoveActivity(shiftLayer, false, command.TrackedCommandInfo);
			((ReadOnlyScheduleDictionary)scheduleDic).MakeEditable();
			scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}
}
