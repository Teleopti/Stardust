using System;
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
	public class RemoveShiftCommandHandler : IHandleCommand<RemoveShiftCommand>
	{
		private readonly IScheduleDayProvider _scheduleDayProvider;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public RemoveShiftCommandHandler(
			IScheduleDayProvider scheduleDayProvider,
			IScheduleDifferenceSaver scheduleDifferenceSaver
			 )
		{
			_scheduleDayProvider = scheduleDayProvider;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(RemoveShiftCommand command)
		{
			var scheduleDic = _scheduleDayProvider.GetScheduleDictionary(command.Date, command.Person);
			var scheduleRange = scheduleDic[command.Person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment != null && personAssignment.DayOff() != null )
			{
				command.ErrorMessages.Add(Resources.CanNotRemoveShiftForAgentWithDayOff);
				return;
			}
			if (personAssignment == null || !personAssignment.MainActivities().Any())
			{
				command.ErrorMessages.Add(Resources.CanNotRemoveShiftForAgentWithEmptySchedule);
				return;
			}

			if (scheduleDay.IsFullDayAbsence())
			{
				command.ErrorMessages.Add(Resources.CanNotRemoveShiftForAgentWithFullDayAbsence);
				return;
			}
			if (personAssignment.DayOff() != null)
			{
				command.ErrorMessages.Add(Resources.CanNotRemoveShiftForAgentWithDayOff);
				return;
			}
			
			personAssignment.ClearMainActivities(false,command.TrackedCommandInfo);
			((ReadOnlyScheduleDictionary)scheduleDic).MakeEditable();
			scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);

		}
	}
}