using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IPersonShiftBagValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingShiftBag(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}