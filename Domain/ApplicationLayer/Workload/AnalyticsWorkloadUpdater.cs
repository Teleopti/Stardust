using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Workload
{
	public class AnalyticsWorkloadUpdater : 
		IHandleEvent<WorkloadChangedEvent>, 
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsWorkloadUpdater));
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsWorkloadRepository _analyticsWorkloadRepository;

		public AnalyticsWorkloadUpdater(IWorkloadRepository workloadRepository,
			IAnalyticsSkillRepository analyticsSkillRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsWorkloadRepository analyticsWorkloadRepository)
		{
			_workloadRepository = workloadRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsWorkloadRepository = analyticsWorkloadRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(WorkloadChangedEvent @event)
		{
			var workload = _workloadRepository.Get(@event.WorkloadId);
			if (workload == null)
			{
				logger.Warn("Workload missing from Application database, aborting.");
				return;
			}

			var businessUnit = getAnalyticsBusinessUnit(@event.LogOnBusinessUnitId);
			var analyticsSkill = getAnalyticsSkill(businessUnit, workload);
			var skill = workload.Skill;
			var queueAdjustments = workload.QueueAdjustments;
			var workloadIsDeleted = (workload as IDeleteTag)?.IsDeleted ?? false;

			var datasourceUpdateDate = workload.UpdatedOn.GetValueOrDefault(AnalyticsDate.Eternity.DateDate);
			var analyticsWorkload = new AnalyticsWorkload
			{
				WorkloadCode = workload.Id.GetValueOrDefault(),
				WorkloadName = workload.Name,
				SkillId = analyticsSkill.SkillId,
				SkillCode = skill.Id.GetValueOrDefault(),
				SkillName = skill.Name,
				TimeZoneId = analyticsSkill.TimeZoneId,
				ForecastMethodCode = skill.SkillType.Id.GetValueOrDefault(),
				ForecastMethodName = skill.SkillType.ForecastSource.ToString(),
				PercentageOffered = queueAdjustments.OfferedTasks.Value,
				PercentageOverflowIn = queueAdjustments.OverflowIn.Value,
				PercentageOverflowOut = queueAdjustments.OverflowOut.Value,
				PercentageAbandoned = queueAdjustments.Abandoned.Value,
				PercentageAbandonedShort = queueAdjustments.AbandonedShort.Value,
				PercentageAbandonedWithinServiceLevel = queueAdjustments.AbandonedWithinServiceLevel.Value,
				PercentageAbandonedAfterServiceLevel = queueAdjustments.AbandonedAfterServiceLevel.Value,
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = datasourceUpdateDate,
				IsDeleted = workloadIsDeleted
			};
			var workloadId = _analyticsWorkloadRepository.AddOrUpdate(analyticsWorkload);

			var existingBridgeQueueWorkloads = _analyticsWorkloadRepository.GetBridgeQueueWorkloads(workloadId);

			IEnumerable<IQueueSource> toBeAdded;
			IEnumerable<AnalyticsBridgeQueueWorkload> toBeDeleted;
			if (workloadIsDeleted)
			{
				toBeAdded = new List<IQueueSource>();
				// Delete all bridge_queue_workload connected to this workload if already deleted
				toBeDeleted = existingBridgeQueueWorkloads;
			}
			else
			{
				toBeAdded =
					workload.QueueSourceCollection.Where(
						queue => existingBridgeQueueWorkloads.All(x => x.QueueId != queue.QueueMartId));
				toBeDeleted = existingBridgeQueueWorkloads.Where(bridge =>
					workload.QueueSourceCollection.All(x => x.QueueMartId != bridge.QueueId));
			}

			foreach (var queue in toBeAdded)
			{
				var bridgeQueueWorkload = new AnalyticsBridgeQueueWorkload
				{
					QueueId = queue.QueueMartId,
					WorkloadId = workloadId,
					SkillId = analyticsSkill.SkillId,
					BusinessUnitId = businessUnit.BusinessUnitId,
					DatasourceUpdateDate = datasourceUpdateDate
				};
				_analyticsWorkloadRepository.AddOrUpdateBridge(bridgeQueueWorkload);
			}
			foreach (var queue in toBeDeleted)
			{
				_analyticsWorkloadRepository.DeleteBridge(workloadId, queue.QueueId);
			}
		}

		private AnalyticsSkill getAnalyticsSkill(AnalyticBusinessUnit businessUnit, IWorkload workload)
		{
			var analyticsSkills = _analyticsSkillRepository.Skills(businessUnit.BusinessUnitId);
			var analyticsSkill = analyticsSkills.FirstOrDefault(s => s.SkillCode == workload.Skill.Id.GetValueOrDefault());
			if (analyticsSkill == null) throw new SkillMissingInAnalyticsException();
			return analyticsSkill;
		}

		private AnalyticBusinessUnit getAnalyticsBusinessUnit(Guid businessUnitId)
		{
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(businessUnitId);
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			return analyticsBusinessUnit;
		}
	}
}