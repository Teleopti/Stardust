using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduledResourcesReadModelReader
	{
		ResourcesFromStorage ForPeriod(DateTimePeriod period, IEnumerable<ISkill> allSkills);
	}
}