using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduledResourcesReadModelStorage : IScheduledResourcesReadModelStorage
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduledResourcesReadModelStorage(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public long AddResources(Guid activityId, bool activityRequiresSeat, string skills, DateTimePeriod period, double resources, double heads)
		{
			return _currentUnitOfWork.Session().CreateSQLQuery("exec ReadModel.AddResources @Activity=:Activity,@ActivityRequiresSeat=:ActivityRequiresSeat,@Skills=:Skills,@PeriodStart=:PeriodStart,@PeriodEnd=:PeriodEnd,@Resources=:Resources,@Heads=:Heads")
			           .SetGuid("Activity", activityId)
			           .SetBoolean("ActivityRequiresSeat", activityRequiresSeat)
			           .SetString("Skills", skills)
			           .SetDateTime("PeriodStart", period.StartDateTime)
			           .SetDateTime("PeriodEnd", period.EndDateTime)
			           .SetDouble("Resources", resources)
			           .SetDouble("Heads", heads)
			           .UniqueResult<long>();
		}

		public void AddSkillEfficiency(long resourceId, Guid skillId, double efficiency)
		{
			_currentUnitOfWork.Session().CreateSQLQuery("exec ReadModel.AddSkillEfficiency @ResourceId=:ResourceId,@SkillId=:SkillId,@Efficiency=:Efficiency")
					   .SetInt64("ResourceId", resourceId)
					   .SetGuid("SkillId", skillId)
					   .SetDouble("Efficiency", efficiency)
					   .ExecuteUpdate();
		}

		public void RemoveSkillEfficiency(long resourceId, Guid skillId, double efficiency)
		{
			_currentUnitOfWork.Session().CreateSQLQuery("exec ReadModel.RemoveSkillEfficiency @ResourceId=:ResourceId,@SkillId=:SkillId,@Efficiency=:Efficiency")
					   .SetInt64("ResourceId", resourceId)
					   .SetGuid("SkillId", skillId)
					   .SetDouble("Efficiency", efficiency)
					   .ExecuteUpdate();
		}

		public long? RemoveResources(Guid activityId, string skills, DateTimePeriod period, double resources, double heads)
		{
			return _currentUnitOfWork.Session().CreateSQLQuery("exec ReadModel.RemoveResources @Activity=:Activity,@Skills=:Skills,@PeriodStart=:PeriodStart,@PeriodEnd=:PeriodEnd,@Resources=:Resources,@Heads=:Heads")
			           .SetGuid("Activity", activityId)
			           .SetString("Skills", skills)
			           .SetDateTime("PeriodStart", period.StartDateTime)
			           .SetDateTime("PeriodEnd", period.EndDateTime)
			           .SetDouble("Resources", resources)
			           .SetDouble("Heads", heads)
					   .UniqueResult<long?>();
		}

		public ResourcesFromStorage ForPeriod(DateTimePeriod period, IEnumerable<ISkill> allSkills)
		{
			var combinations =
				_currentUnitOfWork.Session()
				                  .CreateSQLQuery(
					                  "SELECT Id,Activity,Skills,ActivityRequiresSeat FROM ReadModel.ActivitySkillCombination")
				                  .SetResultTransformer(Transformers.AliasToBean<ActivitySkillCombinationFromStorage>())
				                  .List<ActivitySkillCombinationFromStorage>();

			var resources =
				_currentUnitOfWork.Session()
				                  .CreateSQLQuery(
									  "SELECT Id,ActivitySkillCombinationId,Resources,Heads,PeriodStart,PeriodEnd FROM ReadModel.ScheduledResources WHERE PeriodStart<:Max AND PeriodEnd>:Min")
									  .SetDateTime("Min", period.StartDateTime)
									  .SetDateTime("Max",period.EndDateTime)
				                  .SetResultTransformer(Transformers.AliasToBean<ResourcesForCombinationFromStorage>())
				                  .List<ResourcesForCombinationFromStorage>();
			
			var efficiencies =
				_currentUnitOfWork.Session()
				                  .CreateSQLQuery(
					                  "SELECT e.ParentId,e.SkillId,e.Amount FROM ReadModel.PeriodSkillEfficiencies e INNER JOIN ReadModel.ScheduledResources s ON s.Id=e.ParentId WHERE s.PeriodStart<:Max AND s.PeriodEnd>:Min")
									  .SetDateTime("Min", period.StartDateTime)
									  .SetDateTime("Max", period.EndDateTime)
				                  .SetResultTransformer(Transformers.AliasToBean<SkillEfficienciesFromStorage>())
				                  .List<SkillEfficienciesFromStorage>();

			return new ResourcesFromStorage(resources,combinations,efficiencies,allSkills);
		}
	}
}