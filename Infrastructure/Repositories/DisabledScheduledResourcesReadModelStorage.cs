using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Erik Sundberg: Bug25359
	/// </summary>
	public class DisabledScheduledResourcesReadModelStorage :
		IScheduledResourcesReadModelPersister,
		IScheduledResourcesReadModelReader
	{
		public long AddResources(Guid activityId, bool activityRequiresSeat, string skills, DateTimePeriod period, double resources,
								 double heads)
		{
			return 0;
		}

		public long? RemoveResources(Guid activityId, string skills, DateTimePeriod period, double resources, double heads)
		{
			return null;
		}

		public void AddSkillEfficiency(long resourceId, Guid skillId, double efficiency)
		{
		}

		public void RemoveSkillEfficiency(long resourceId, Guid skillId, double efficiency)
		{
		}

		public void ActivityUpdated(Guid activityId, bool requiresSeat)
		{
		}

		public ResourcesFromStorage ForPeriod(DateTimePeriod period, IEnumerable<ISkill> allSkills)
		{
			return new ResourcesFromStorage(new Collection<ResourcesForCombinationFromStorage>(),
			                                new Collection<ActivitySkillCombinationFromStorage>(),
			                                new Collection<SkillEfficienciesFromStorage>(),
			                                new Collection<ISkill>());
		}
	}
}
