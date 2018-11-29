using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveActivityCommandHandler : IHandleCommand<MoveActivityCommand>
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _userTimezone;

		public MoveActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId, 
										IProxyForId<IPerson> personForId, 
										IProxyForId<IActivity> activityForId, 
										ICurrentScenario currentScenario, 
										IUserTimeZone userTimezone)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_personForId = personForId;
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_userTimezone = userTimezone;
		}

		public void Handle(MoveActivityCommand command)
		{
			var assignedAgent = _personForId.Load(command.AgentId);
			var activity = _activityForId.Load(command.ActivityId);
			var currentScenario = _currentScenario.Current();
			var currentDate = command.ScheduleDate;
			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = currentDate,
				Person = assignedAgent,
				Scenario = currentScenario
			});

			if (personAssignment == null)
				throw new InvalidOperationException(string.Format("Person assigment is not found. Date: {0} PersonId: {1} Scenario: {2} ", currentDate, assignedAgent.Id, currentScenario.Description));

			var timeZone = _userTimezone.TimeZone();
			var oldStartTimeUtc = TimeZoneHelper.ConvertToUtc(command.OldStartTime, timeZone);
			var newStartTimeUtc = TimeZoneHelper.ConvertToUtc(command.NewStartTime, timeZone);
			personAssignment.MoveActivityAndSetHighestPriority(activity, oldStartTimeUtc, newStartTimeUtc, TimeSpan.FromMinutes(command.OldProjectionLayerLength), command.TrackedCommandInfo);
		}
	}
}