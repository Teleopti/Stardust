using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	[EnabledBy(Toggles.ETL_FasterIndexMaintenance_38847)]
	public class IndexMaintenanceStardustHandler :
		IHandleEvent<IndexMaintenanceEvent>,
		IRunOnStardust
	{
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IndexMaintenanceStardustHandler));

		public IndexMaintenanceStardustHandler(IIndexMaintenanceRepository indexMaintenanceRepository)
		{
			_indexMaintenanceRepository = indexMaintenanceRepository;
		}

		[TenantScope]
		public virtual void Handle(IndexMaintenanceEvent @event)
		{
			logger.Debug($"Consuming event for {nameof(IndexMaintenanceEvent)}");
			_indexMaintenanceRepository.PerformIndexMaintenanceForAll();
		}
	}
}