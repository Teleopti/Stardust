using System;
using System.Threading;
using log4net;
using Teleopti.Analytics.Etl.Common.TenantHeartbeat;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;
		private readonly TenantHearbeatEventPublisher _tenantHearbeatEventPublisher;

		public EtlService(EtlJobStarter etlJobStarter, TenantHearbeatEventPublisher tenantHearbeatEventPublisher)
		{
			_etlJobStarter = etlJobStarter;
			_tenantHearbeatEventPublisher = tenantHearbeatEventPublisher;
			_timer = new Timer(tick, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
		}

		public void Start(DateTime serviceStartTime, Action stopService)
		{
			_etlJobStarter.Initialize(serviceStartTime, stopService);
			_timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
		}

		void tick(object state)
		{
			var stop = false;

			log.Debug("Tick");
			
			try
			{
				_tenantHearbeatEventPublisher.Tick();
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