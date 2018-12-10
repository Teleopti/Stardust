using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddDayOffCommandHandler : IHandleCommand<AddDayOffCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public AddDayOffCommandHandler(ICurrentScenario currentScenario,
			IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_currentScenario = currentScenario;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_scheduleStorage = scheduleStorage;
		}


		public void Handle(AddDayOffCommand command)
		{
			var scenario = _currentScenario.Current();

			var datePeriod = new DateTimePeriod(DateTime.SpecifyKind(command.StartDate.Date, DateTimeKind.Utc), DateTime.SpecifyKind(command.EndDate.Date.AddDays(1), DateTimeKind.Utc));
			var loadOptions = new ScheduleDictionaryLoadOptions(false, false);
			var scheduleDic = _scheduleStorage.FindSchedulesForPersons(scenario,
				new[] { command.Person },
				loadOptions,
				datePeriod,
				new[] { command.Person },
				false);
			var scheduleRange = scheduleDic[command.Person];
			var days = new DateOnlyPeriod(command.StartDate, command.EndDate).DayCollection();
		
			foreach (var day in days)
			{
				var scheduleDay = scheduleRange.ScheduledDay(day);
				scheduleDay
					.PersonAssignment(true)
					.SetDayOff(command.Template, false, command.TrackedCommandInfo);
				scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			}
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);

		}
	}
}