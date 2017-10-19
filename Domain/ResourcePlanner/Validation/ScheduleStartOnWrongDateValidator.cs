using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class ScheduleStartOnWrongDateValidator : IScheduleValidator
	{
		public void FillResult(ValidationResult validationResult, IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			if (schedules == null)
				return;
			throw new System.NotImplementedException();
		}
	}
}