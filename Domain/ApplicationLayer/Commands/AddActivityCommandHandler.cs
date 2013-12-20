using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;

		public AddActivityCommandHandler(
			IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository,
			ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId,
			IUserTimeZone timeZone)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
				{
					Date = command.Date,
					Scenario = _currentScenario.Current(),
					Person = _personForId.Load(command.PersonId)
				});
			if (personAssignment == null)
				return;
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));
			personAssignment.AddActivity(activity, period);
		}
	}
}