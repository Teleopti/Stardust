using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduledResourcesReadModelStorage
	{
		void AddResources(Guid activityId, bool activityRequiresSeat, string skills, DateTimePeriod period, double resources, double heads);
		void RemoveResources(Guid activityId, string skills, DateTimePeriod period, double resources, double heads);
	}
}