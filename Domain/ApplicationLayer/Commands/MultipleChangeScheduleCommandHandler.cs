using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftEditor
{
	public class MultipleChangeScheduleCommandHandler : IHandleCommand<MultipleChangeScheduleCommand>
	{
		private readonly IReplaceLayerInSchedule _replaceLayerInSchedule;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentDataSource _currentDataSource;

		public MultipleChangeScheduleCommandHandler(
			IReplaceLayerInSchedule replaceLayerInSchedule,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IEventPublisher eventPublisher,
			ICurrentDataSource currentDataSource)
		{
			_replaceLayerInSchedule = replaceLayerInSchedule;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_eventPublisher = eventPublisher;
			_currentDataSource = currentDataSource;
		}


		public void Handle(MultipleChangeScheduleCommand command)
		{
			var scheduleDic = command.ScheduleDictionary;
			var scheduleRange = scheduleDic[command.Person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();

			foreach (var cd in command.Commands)
			{
				switch (cd.GetType().Name)
				{
					case nameof(ChangeActivityTypeCommand):
						handle(scheduleDay, cd as ChangeActivityTypeCommand);
						break;

					case nameof(AddActivityCommandSimply):
						handle(personAssignment, cd as AddActivityCommandSimply);
						break;

					case nameof(AddPersonalActivityCommandSimply):
						handle(personAssignment, cd as AddPersonalActivityCommandSimply);
						break;
					case nameof(AddOvertimeActivityCommandSimply):
						handle(personAssignment, cd as AddOvertimeActivityCommandSimply);
						break;
				}
			}

			raiseEvent(personAssignment, command);

			scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}

		private void handle(IScheduleDay scheduleDay, ChangeActivityTypeCommand command)
		{
			var period = command.ShiftLayer.Period;
			_replaceLayerInSchedule.Replace(scheduleDay, command.ShiftLayer, command.Activity, period);
		}

		private void handle(IPersonAssignment personAssignment, AddActivityCommandSimply command)
		{
			personAssignment.AddActivity(command.Activity, command.Period, true);
		}
		private void handle(IPersonAssignment personAssignment, AddPersonalActivityCommandSimply command)
		{
			personAssignment.AddPersonalActivity(command.Activity, command.Period);
		}

		private void handle(IPersonAssignment personAssignment, AddOvertimeActivityCommandSimply command)
		{
			personAssignment.AddOvertimeActivity(command.Activity, command.Period, command.MultiplicatorDefinitionSet);
		}

		private void raiseEvent(IPersonAssignment personAssignment, MultipleChangeScheduleCommand command)
		{
			_eventPublisher.Publish(new ScheduleChangedEvent
			{
				Date = personAssignment.Date.Date,
				PersonId = personAssignment.Person.Id.Value,
				StartDateTime = personAssignment.Period.StartDateTime,
				EndDateTime = personAssignment.Period.EndDateTime,
				ScenarioId = personAssignment.Scenario.Id.Value,
				CommandId = command.TrackedCommandInfo?.TrackId ?? Guid.Empty,
				LogOnBusinessUnitId = personAssignment.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.Value,
				InitiatorId = command.TrackedCommandInfo?.OperatedPersonId ?? Guid.Empty,
				LogOnDatasource = _currentDataSource.CurrentName()
			});
		}
	}
}
