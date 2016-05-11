using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface ICancelAbsenceRequestCommandValidator
	{
		bool ValidateCommand(IPersonRequest personRequest, CancelAbsenceRequestCommand command, IAbsenceRequest absenceRequest, ICollection<IPersonAbsence> personAbsences);
	}
}