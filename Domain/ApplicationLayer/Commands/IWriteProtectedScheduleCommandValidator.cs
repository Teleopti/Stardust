using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IWriteProtectedScheduleCommandValidator
	{
		bool ValidateCommand(DateTime date, IPerson agent, IErrorAttachedCommand command, out string errorMessage);
	}
}