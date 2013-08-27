﻿using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;

		public RemovePersonAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository)
		{
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence) _personAbsenceRepository.Load(command.PersonAbsenceId);

			personAbsence.RemovePersonAbsence();

			_personAbsenceRepository.Remove(personAbsence);
		}
	}
}