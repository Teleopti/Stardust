using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduledResourcesReadModelStorage
	{
		long AddResources(Guid activityId, bool activityRequiresSeat, string skills, DateTimePeriod period, double resources, double heads);
		long? RemoveResources(Guid activityId, string skills, DateTimePeriod period, double resources, double heads);
		void AddSkillEfficiency(long resourceId, Guid skillId, double efficiency);
		void RemoveSkillEfficiency(long resourceId, Guid skillId, double efficiency);
		ResourcesFromStorage ForPeriod(DateTimePeriod period, IEnumerable<ISkill> allSkills);
		void ActivityUpdated(Guid activityId, bool requiresSeat);
	}
}