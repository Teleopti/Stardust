using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{

	[EnabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class AddPersonalActivityCommandHandler : IHandleCommand<AddPersonalActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public AddPersonalActivityCommandHandler(
			ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId,
			IUserTimeZone timeZone, IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(AddPersonalActivityCommand command)
		{
			var activity = _activityForId.Load(command.PersonalActivityId);
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var dic = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			
			var scheduleRange = dic[person];
			
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var schedulePreviousDay = scheduleRange.ScheduledDay(command.Date.AddDays(-1));
			command.ErrorMessages = new List<string>();

			var personAssignment = scheduleDay.PersonAssignment();
			var personAssignmentOfPreviousDay = schedulePreviousDay.PersonAssignment();
			if (personAssignmentOfPreviousDay != null && personAssignmentOfPreviousDay.Period.EndDateTime >= period.StartDateTime)
			{
				command.ErrorMessages.Add(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
				return;
			}

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.FailedMessageForAddingActivity);
			}
			else
			{
				scheduleDay.CreateAndAddPersonalActivity(activity, period, false);
				dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
				_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
			}
		}
	}

	[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class AddPersonalActivityCommandHandlerNoDeltas : IHandleCommand<AddPersonalActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;

		public AddPersonalActivityCommandHandlerNoDeltas(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId, IUserTimeZone timeZone)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
		}

		public void Handle(AddPersonalActivityCommand command)
		{
			var activity = _activityForId.Load(command.PersonalActivityId);
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			command.ErrorMessages = new List<string>();
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var personAssignmentOfPreviousDay = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date.AddDays(-1),
				Scenario = scenario,
				Person = person
			});

			if (personAssignmentOfPreviousDay != null && personAssignmentOfPreviousDay.Period.EndDateTime >= period.StartDateTime)
			{
				command.ErrorMessages.Add(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
				return;
			}

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.FailedMessageForAddingActivity);
			}
			else
			{
				personAssignment.AddPersonalActivity(activity, period, false, command.TrackedCommandInfo);
			}
		}
	}

}