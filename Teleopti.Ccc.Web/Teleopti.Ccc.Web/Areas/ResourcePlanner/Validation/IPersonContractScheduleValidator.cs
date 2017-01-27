using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IPersonContractScheduleValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingContractSchedule(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}