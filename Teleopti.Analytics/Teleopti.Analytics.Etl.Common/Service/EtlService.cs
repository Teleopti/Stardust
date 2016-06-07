using System;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;
		private readonly TenantTickEventPublisher _tenantTickEventPublisher;
		private readonly IndexMaintenancePublisher _indexMaintenancePublisher;

		public EtlService(EtlJobStarter etlJobStarter, TenantTickEventPublisher tenantTickEventPublisher, IndexMaintenancePublisher indexMaintenancePublisher)
		{
			_etlJobStarter = etlJobStarter;
			_tenantTickEventPublisher = tenantTickEventPublisher;
			_indexMaintenancePublisher = indexMaintenancePublisher;
			_timer = new Timer(tick, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
		}

		public void Start(DateTime serviceStartTime, Action stopService)
		{
			_etlJobStarter.Initialize(serviceStartTime, stopService);
			_timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
			_indexMaintenancePublisher.Start();
		}

		void tick(object state)
		{
			var stop = false;

			log.Debug("Tick");
			
			try
			{
				_tenantTickEventPublisher.EnsurePublishings();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				log.Error("Exception occurred invoking TenantHearbeatEventPublisher", ex);
			}

			try
			{
				stop = !_etlJobStarter.Tick();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				log.Error("Exception occurred invoking EtlJobStarter", ex);
			}

			log.DebugFormat("stop: {0}", stop);
			if (!stop)
			{
				log.Debug("Starting timer");
				if (_timer != null)
				{
					_timer.Change(TimeSpan.FromSeconds(10),TimeSpan.FromMilliseconds(-1));
				}
				log.Debug("Timer started");
			}

		}

		public void Dispose()
		{
			log.Info("The service is stopping.");
			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}
	}
}