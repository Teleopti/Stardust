using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IPersonPartTimePercentageValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingPartTimePercentage(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}