using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftEditor
{
	public class MultipleChangeScheduleCommandHandler : IHandleCommand<MultipleChangeScheduleCommand>
	{
		private readonly IReplaceLayerInSchedule _replaceLayerInSchedule;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly ILoggedOnUser _loggedOnUser;
		public MultipleChangeScheduleCommandHandler(IReplaceLayerInSchedule replaceLayerInSchedule, IScheduleDifferenceSaver scheduleDifferenceSaver, ILoggedOnUser loggedOnUser)
		{
			_replaceLayerInSchedule = replaceLayerInSchedule;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_loggedOnUser = loggedOnUser;
		}


		public void Handle(MultipleChangeScheduleCommand command)
		{
			var scheduleDic = command.ScheduleDictionary;
			var scheduleRange = scheduleDic[command.Person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);

			foreach (var cd in command.Commands)
			{
				switch (cd.GetType().Name)
				{
					case nameof(ChangeActivityTypeCommand):
						handle(scheduleDay, cd as ChangeActivityTypeCommand);
						break;

					case nameof(AddActivityCommandSimply):
						handle(scheduleDay, cd as AddActivityCommandSimply);
						break;

					case nameof(AddPersonalActivityCommandSimply):
						handle(scheduleDay, cd as AddPersonalActivityCommandSimply);
						break;
					case nameof(AddOvertimeActivityCommandSimply):
						handle(scheduleDay, cd as AddOvertimeActivityCommandSimply);
						break;

				}
			}

			scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);

		}

		private void handle(IScheduleDay scheduleDay, ChangeActivityTypeCommand command)
		{
			var period = command.ShiftLayer.Period;
			_replaceLayerInSchedule.Replace(scheduleDay, command.ShiftLayer, command.Activity, period);
		}

		private void handle(IScheduleDay scheduleDay, AddActivityCommandSimply command)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			personAssignment.AddActivity(command.Activity, command.Period);
		}
		private void handle(IScheduleDay scheduleDay, AddPersonalActivityCommandSimply command)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			personAssignment.AddPersonalActivity(command.Activity, command.Period);
		}

		private void handle(IScheduleDay scheduleDay, AddOvertimeActivityCommandSimply command)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			personAssignment.AddOvertimeActivity(command.Activity, command.Period, command.MultiplicatorDefinitionSet);
		}
	}
}
