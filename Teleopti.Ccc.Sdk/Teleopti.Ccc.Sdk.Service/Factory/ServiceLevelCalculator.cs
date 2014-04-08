using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class ServiceLevelCalculator : IServiceLevelCalculator
	{
		public Percent EstimatedServiceLevel(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
		{
			return SkillStaffPeriodHelper.EstimatedServiceLevel(skillStaffPeriods);
		}
	}
}