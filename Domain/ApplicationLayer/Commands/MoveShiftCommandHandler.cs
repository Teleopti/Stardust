using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftCommandHandler : IHandleCommand<MoveShiftCommand>
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly ICurrentScenario _currentScenario;


		public MoveShiftCommandHandler(IProxyForId<IPerson> personForId, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId, ICurrentScenario currentScenario)
		{
			_personForId = personForId;
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveShiftCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var currentScenario = _currentScenario.Current();
			var currentDate = command.ScheduleDate;

			var personAss = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = currentDate,
				Person = person,
				Scenario = currentScenario
			});

			if (personAss == null)
			{
				command.ErrorMessages = new List<string> { Resources.PersonAssignmentIsNotValid };
				return;
			}

			personAss.MoveAllActivitiesAndKeepOriginalPriority(command.NewStartTimeInUtc, command.TrackedCommandInfo);
		}
	}
}