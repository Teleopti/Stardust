using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddOvertimeActivityCommandHandler:IHandleCommand<AddOvertimeActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IProxyForId<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetForId;

		public AddOvertimeActivityCommandHandler(IProxyForId<IActivity> activityForId, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IPerson> personForId, IProxyForId<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetForId)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_multiplicatorDefinitionSetForId = multiplicatorDefinitionSetForId;
		}


		public void Handle(AddOvertimeActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = _personForId.Load(command.PersonId);			
			var scenario = _currentScenario.Current();
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetForId.Load(command.MultiplicatorDefinitionSetId);

			command.ErrorMessages = new List<string>();

			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			if(personAssignment == null)
			{
				personAssignment = new PersonAssignment(person,scenario,command.Date);
				_personAssignmentRepository.Add(personAssignment);
			}

			personAssignment.AddOvertimeActivity(activity,command.Period,multiplicatorDefinitionSet, false, command.TrackedCommandInfo);
		}
	}
}
