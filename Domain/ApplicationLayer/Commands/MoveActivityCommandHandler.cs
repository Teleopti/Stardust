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

		public MoveActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId, 
																		IProxyForId<IPerson> personForId, 
																		ICurrentScenario currentScenario)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_personForId = personForId;
			_currentScenario = currentScenario;
		}

		public void Handle(MoveActivityCommand command)
		{
			var assignedAgent = _personForId.Load(command.AgentId);
			var currentScenario = _currentScenario.Current();
			var currentDate = command.Date;
			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = currentDate,
				Person = assignedAgent,
				Scenario = currentScenario
			});

			if (personAssignment == null)
				throw new InvalidOperationException(string.Format("Person assigment is not found. Date: {0} PersonId: {1} Scenario: {2} ", currentDate, assignedAgent.Id, currentScenario.Description));
			
			var newStartTimeLocal = personAssignment.Date.Date.Add(command.NewStartTime);
			var startTimeUtc = new DateTime(newStartTimeLocal.Ticks, DateTimeKind.Utc);

			var layerWithSpecificActivity = personAssignment.ShiftLayers.Single(x => x.Payload.Id.Value == command.ActivityId && x.Period.StartDateTime == command.OldStartTime);
			personAssignment.MoveActivity(layerWithSpecificActivity, new DateTimePeriod(startTimeUtc, startTimeUtc.Add(command.OldProjectionLayerLength)));
		}
	}
}