using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	[EnabledBy(Toggles.ETL_FasterIndexMaintenance_38847)]
	public class IndexMaintenanceStardustHandler :
		IHandleEvent<EtlNightlyEndEvent>,
		IRunOnStardust
	{
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;
		private readonly IAllTenantEtlSettings _allTenantEtlSettings;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IndexMaintenanceStardustHandler));

		public IndexMaintenanceStardustHandler(IIndexMaintenanceRepository indexMaintenanceRepository, IAllTenantEtlSettings allTenantEtlSettings)
		{
			_indexMaintenanceRepository = indexMaintenanceRepository;
			_allTenantEtlSettings = allTenantEtlSettings;
		}

		[TenantScope]
		public virtual void Handle(IndexMaintenanceStardustEvent @event)
		{
			_indexMaintenanceRepository.PerformIndexMaintenanceForAll();
		}

		[TenantScope]
		public virtual void Handle(EtlNightlyEndEvent @event)
		{
			var settings = _allTenantEtlSettings.Get(@event.LogOnDatasource);

			logger.Debug($"Consuming event for {nameof(EtlNightlyEndEvent)}");
			logger.Debug($"{nameof(settings.RunIndexMaintenance)} is {settings.RunIndexMaintenance} for tenant {@event.LogOnDatasource}");

			if (settings.RunIndexMaintenance)
			{
				_indexMaintenanceRepository.PerformIndexMaintenanceForAll();
			}
		}
	}
}