using System;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;
		private readonly TenantTickEventPublisher _tenantTickEventPublisher;
		private readonly IIndexMaintenanceHangfireEventPublisher _indexMaintenanceHangfireEventPublisher;
		private readonly IRecurringEventPublisher _recurringEventPublisher;
		private DateTime? lastTenantRecurringJobPublishing;
		private INow _now;

		public EtlService(EtlJobStarter etlJobStarter, TenantTickEventPublisher tenantTickEventPublisher, IIndexMaintenanceHangfireEventPublisher indexMaintenanceHangfireEventPublisher, IRecurringEventPublisher recurringEventPublisher, INow now)
		{
			_etlJobStarter = etlJobStarter;
			_tenantTickEventPublisher = tenantTickEventPublisher;
			_indexMaintenanceHangfireEventPublisher = indexMaintenanceHangfireEventPublisher;
			_recurringEventPublisher = recurringEventPublisher;
			_now = now;
			_timer = new Timer(tick, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
		}

		public void EnsureSystemWideRecurringJobs()
		{
			_recurringEventPublisher.StopPublishingAll();
			_recurringEventPublisher.PublishHourly(new CleanFailedQueue());
		}

		public void Start(DateTime serviceStartTime, Action stopService)
		{
			EnsureSystemWideRecurringJobs();
			_etlJobStarter.Initialize(serviceStartTime, stopService);
			_timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
		}

		void tick(object state)
		{
			var stop = false;

			log.Debug("Tick");
			
			EnsureTenantRecurringJobs();

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
					_timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
				}
				log.Debug("Timer started");
			}
		}

		public void EnsureTenantRecurringJobs()
		{
			try
			{
				if (lastTenantRecurringJobPublishing == null ||
					_now.UtcDateTime().Subtract(lastTenantRecurringJobPublishing.GetValueOrDefault()).TotalMinutes >= 10)
				{
					_tenantTickEventPublisher.RemovePublishingsOfRemovedTenants();
					_tenantTickEventPublisher.PublishRecurringJobs();
					_indexMaintenanceHangfireEventPublisher.PublishRecurringJobs();
					lastTenantRecurringJobPublishing = _now.UtcDateTime();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				log.Error("Exception occurred invoking TenantHearbeatEventPublisher", ex);
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