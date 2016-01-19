using System;
using System.Timers;
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
		}

		public void Start(DateTime serviceStartTime, Action stopService)
		{
			_etlJobStarter.Initialize(serviceStartTime, stopService);
			_timer = new Timer(10000);
			_timer.Elapsed += tick;
			_timer.Start();
		}

		void tick(object sender, ElapsedEventArgs e)
		{
			var stop = false;

			log.Debug("Tick");
			_timer.Stop();
			log.Debug("Timer stopped");

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
				_timer.Start();
				log.Debug("Timer started");
			}

		}

		public void Dispose()
		{
			log.Info("The service is stopping.");
			if (_timer != null)
			{
				_timer.Stop();
				_timer.Dispose();
			}
		}
	}
}