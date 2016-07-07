using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveActivityCommandHandler:IHandleCommand<RemoveActivityCommand>
	{	
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;

		public RemoveActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IPerson> personForId)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
		}

		public void Handle(RemoveActivityCommand command)
		{
			
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			command.ErrorMessages = new List<string>();

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.PersonAssignmentIsNotValid);
				return;
			}
			
			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages.Add(Resources.NoShiftsFound);
				return;
			}

			var mainShiftLayer = shiftLayer as MainShiftLayer;

			if (!(shiftLayer is MainShiftLayer) && !(shiftLayer is PersonalShiftLayer))
			{
				command.ErrorMessages.Add(Resources.CannotDeleteSelectedActivities);
				return;
			}

			var minOrderIndex = personAssignment.ShiftLayers.Min(layer =>
			{
				var layerAsMain = layer as MainShiftLayer;
				return layerAsMain?.OrderIndex ?? int.MaxValue;
			});

			if (mainShiftLayer != null && mainShiftLayer.OrderIndex == minOrderIndex)
			{
				command.ErrorMessages.Add(Resources.CannotDeleteBaseActivity);
				return;
			}

			personAssignment.RemoveActivity(shiftLayer, false, command.TrackedCommandInfo);		
		}
	}
}
