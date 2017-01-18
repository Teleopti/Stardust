using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestValidatorProvider
	{
		IEnumerable<IAbsenceRequestValidator> GetValidatorList(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);

		IEnumerable<IAbsenceRequestValidator> GetValidatorList(RequestValidatorsFlag validator);
	}
}
