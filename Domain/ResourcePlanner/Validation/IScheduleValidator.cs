using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IScheduleValidator
	{
		void FillResult(ValidationResult validationResult, IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period);
	}
}