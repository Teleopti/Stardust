using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IPersonSkillValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingSkill(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}