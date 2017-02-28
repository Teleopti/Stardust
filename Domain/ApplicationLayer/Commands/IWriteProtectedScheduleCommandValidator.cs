using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IWriteProtectedScheduleCommandValidator
	{
		bool ValidateCommand(DateTime date, IPerson agent, IErrorAttachedCommand command);
	}
}