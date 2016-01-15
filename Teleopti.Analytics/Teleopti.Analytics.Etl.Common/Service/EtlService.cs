using System;
using System.Timers;
using log4net;
using Teleopti.Analytics.Etl.Common.TickEvent;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;
		private readonly HourlyTickEventPublisher _hourlyTickEventPublisher;

		public EtlService(EtlJobStarter etlJobStarter, HourlyTickEventPublisher hourlyTickEventPublisher)
		{
			_etlJobStarter = etlJobStarter;
			_hourlyTickEventPublisher = hourlyTickEventPublisher;
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
				_hourlyTickEventPublisher.Tick();
			}
			catch (Exception ex)
			{
				log.Error("Exception occurred invoking HourlyTickEventPublisher", ex);
			}

			try
			{
				stop = !_etlJobStarter.Tick();
			}
			catch (Exception ex)
			{
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