using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ChangeShiftCategoryCommandHandler : IHandleCommand<ChangeShiftCategoryCommand>
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;

		public ChangeShiftCategoryCommandHandler(IShiftCategoryRepository shiftCategoryRepository, IProxyForId<IPerson> personForId, ICurrentScenario currentScenario, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_personForId = personForId;
			_currentScenario = currentScenario;
			_personAssignmentRepository = personAssignmentRepository;
		}

		public void Handle(ChangeShiftCategoryCommand command)
		{
			command.ErrorMessages = new List<string>();
			var shiftCategory = _shiftCategoryRepository.Get(command.ShiftCategoryId);

			if (shiftCategory == null)
			{
				command.ErrorMessages.Add(Resources.ShiftCategories);
				return;
			}

			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.PersonAssignmentIsNotValid);
				return;
			}

			if (!personAssignment.MainActivities().Any())
			{
				command.ErrorMessages.Add(Resources.NoShiftsFound);
				return;
			}

			personAssignment.SetShiftCategory(shiftCategory, false, command.TrackedCommandInfo);
		}
	}
}
