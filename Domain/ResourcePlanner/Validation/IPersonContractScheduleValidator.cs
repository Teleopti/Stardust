using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IPersonContractScheduleValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingContractSchedule(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}