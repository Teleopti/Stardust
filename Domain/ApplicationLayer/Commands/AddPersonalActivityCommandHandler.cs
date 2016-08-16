using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddPersonalActivityCommandHandler : IHandleCommand<AddPersonalActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;

		public AddPersonalActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId, IUserTimeZone timeZone)
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