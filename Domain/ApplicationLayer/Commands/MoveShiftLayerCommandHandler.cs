using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveShiftLayerCommandHandler : IHandleCommand<MoveShiftLayerCommand>
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly ICurrentScenario _currentScenario;

		public MoveShiftLayerCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId,
			IProxyForId<IPerson> personForId, ICurrentScenario currentScenario)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_personForId = personForId;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveShiftLayerCommand command)
		{
			var assignedAgent = _personForId.Load(command.AgentId);
			var currentScenario = _currentScenario.Current();
			var currentDate = command.ScheduleDate;

			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = currentDate,
				Person = assignedAgent,
				Scenario = currentScenario
			});

			if (personAssignment == null)
			{
				command.ErrorMessages = new List<string> { Resources.PersonAssignmentIsNotValidDot };
				return;
			}

			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages = new List<string> { Resources.NoShiftsFound };
				return;
			}
			personAssignment.MoveActivityAndKeepOriginalPriority(shiftLayer, command.NewStartTimeInUtc, command.TrackedCommandInfo);
		}
	}
}