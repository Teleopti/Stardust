using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveDayOffCommandHandler : IHandleCommand<RemoveDayOffCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public RemoveDayOffCommandHandler(ICurrentScenario currentScenario,
			IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_currentScenario = currentScenario;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_scheduleStorage = scheduleStorage;
		}


		public void Handle(RemoveDayOffCommand command)
		{
			var scenario = _currentScenario.Current();

			var datePeriod = new DateTimePeriod(DateTime.SpecifyKind(command.Date.Date, DateTimeKind.Utc), DateTime.SpecifyKind(command.Date.Date, DateTimeKind.Utc));
			var loadOptions = new ScheduleDictionaryLoadOptions(false, false);
			var scheduleDic = _scheduleStorage.FindSchedulesForPersons(scenario,
				new[] { command.Person },
				loadOptions,
				datePeriod,
				new[] { command.Person },
				false);
			var scheduleRange = scheduleDic[command.Person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			scheduleDay.DeleteDayOff(command.TrackedCommandInfo);
			scheduleDic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}
}