using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveActivityCommandHandler : IHandleCommand<MoveActivityCommand>
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly ICurrentScenario _currentScenario;

		public MoveActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId, IProxyForId<IPerson> personForId, ICurrentScenario currentScenario)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_personForId = personForId;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveActivityCommand command)
		{
			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Person = _personForId.Load(command.AgentId),
				Scenario = _currentScenario.Current()
			});
			var layerWithSpecificActivity = personAssignment.ShiftLayers.Single(x => x.Payload.Id.Value == command.ActivityId);

			var layerToMoveStartOriginal = layerWithSpecificActivity.Period.StartDateTime;
			var layerToMoveEndOriginal = layerWithSpecificActivity.Period.EndDateTime;
			var newStart = layerToMoveStartOriginal.Date.Add(command.NewStartTime);
			var newEnd = newStart.Add(layerToMoveEndOriginal - layerToMoveStartOriginal);

			personAssignment.RemoveActivity(layerWithSpecificActivity);
			personAssignment.AddActivity(layerWithSpecificActivity.Payload, new DateTimePeriod(newStart, newEnd));
		}
	}
}