using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class IndexMaintenanceStardustHandler :
		IHandleEvent<IndexMaintenanceEvent>,
		IRunOnStardust
	{
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IndexMaintenanceStardustHandler));

		public IndexMaintenanceStardustHandler(IIndexMaintenanceRepository indexMaintenanceRepository, IStardustJobFeedback stardustJobFeedback)
		{
			_indexMaintenanceRepository = indexMaintenanceRepository;
			_stardustJobFeedback = stardustJobFeedback;
		}

		[TenantScope]
		public virtual void Handle(IndexMaintenanceEvent @event)
		{
			logger.Debug($"Consuming event for {nameof(IndexMaintenanceEvent)} for {@event.LogOnDatasource}");
			performIndexMaintenance(DatabaseEnum.Application);
			performIndexMaintenance(DatabaseEnum.Analytics);
			performIndexMaintenance(DatabaseEnum.Agg);
		}

		private void performIndexMaintenance(DatabaseEnum database)
		{
			_stardustJobFeedback.SendProgress?.Invoke($"Index Maintenance {database} Started");
			_indexMaintenanceRepository.PerformIndexMaintenance(database);
			_stardustJobFeedback.SendProgress?.Invoke($"Index Maintenance {database} Finished");
		}
	}
}