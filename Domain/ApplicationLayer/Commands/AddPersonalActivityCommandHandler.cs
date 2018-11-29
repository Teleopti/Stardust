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
	public class AddPersonalActivityCommandHandler : IHandleCommand<AddPersonalActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _timeZone;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public AddPersonalActivityCommandHandler(
			ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId,
			IUserTimeZone timeZone, IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_timeZone = timeZone;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(AddPersonalActivityCommand command)
		{
			var activity = _activityForId.Load(command.PersonalActivityId);
			var person = command.Person;
			var scenario = _currentScenario.Current();

			var loadPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime.AddDays(-1), _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var dic = _scheduleStorage.FindSchedulesForPersons(scenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), loadPeriod, new[] { person }, false);
			var scheduleRange = dic[person];

			command.ErrorMessages = new List<string>();
		
			var schedulePreviousDay = scheduleRange.ScheduledDay(command.Date.AddDays(-1));
			var personAssignmentOfPreviousDay = schedulePreviousDay.PersonAssignment();
			if (personAssignmentOfPreviousDay != null && !personAssignmentOfPreviousDay.ShiftLayers.IsEmpty() && personAssignmentOfPreviousDay.Period.EndDateTime >= period.StartDateTime)
			{
				command.ErrorMessages.Add(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
				return;
			}

			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			scheduleDay.CreateAndAddPersonalActivity(activity, period, false, command.TrackedCommandInfo);
			dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);

		}
	}

}