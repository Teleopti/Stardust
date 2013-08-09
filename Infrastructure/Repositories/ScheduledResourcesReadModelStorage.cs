using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduledResourcesReadModelStorage : IScheduledResourcesReadModelStorage
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public ScheduledResourcesReadModelStorage(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void AddResources(Guid activityId, bool activityRequiresSeat, string skills, DateTimePeriod period, double resources, double heads)
		{
			_unitOfWork.Session().CreateSQLQuery("exec ReadModel.AddResources @Activity=:Activity,@ActivityRequiresSeat=:ActivityRequiresSeat,@Skills=:Skills,@PeriodStart=:PeriodStart,@PeriodEnd=:PeriodEnd,@Resources=:Resources,@Heads=:Heads")
			           .SetGuid("Activity", activityId)
			           .SetBoolean("ActivityRequiresSeat", activityRequiresSeat)
			           .SetString("Skills", skills)
			           .SetDateTime("PeriodStart", period.StartDateTime)
			           .SetDateTime("PeriodEnd", period.EndDateTime)
			           .SetDouble("Resources", resources)
			           .SetDouble("Heads", heads)
			           .ExecuteUpdate();
		}

		public void RemoveResources(Guid activityId, string skills, DateTimePeriod period, double resources, double heads)
		{
			_unitOfWork.Session().CreateSQLQuery("exec ReadModel.RemoveResources @Activity=:Activity,@Skills=:Skills,@PeriodStart=:PeriodStart,@PeriodEnd=:PeriodEnd,@Resources=:Resources,@Heads=:Heads")
			           .SetGuid("Activity", activityId)
			           .SetString("Skills", skills)
			           .SetDateTime("PeriodStart", period.StartDateTime)
			           .SetDateTime("PeriodEnd", period.EndDateTime)
			           .SetDouble("Resources", resources)
			           .SetDouble("Heads", heads)
			           .ExecuteUpdate();
		}
	}
}