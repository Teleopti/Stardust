using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
	public interface IServiceLevelCalculator
	{
		Percent EstimatedServiceLevel(IEnumerable<ISkillStaffPeriod> skillStaffPeriods);
	}
}