using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestValidatorProvider
	{
		IEnumerable<IAbsenceRequestValidator> GetValidatorList(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);

		IEnumerable<IAbsenceRequestValidator> GetValidatorList(IPersonRequest personRequest, RequestValidatorsFlag validator);
	}
}
