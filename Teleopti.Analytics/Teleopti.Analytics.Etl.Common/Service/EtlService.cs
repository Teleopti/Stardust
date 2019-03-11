using System;
using System.Data.SqlClient;
using System.Threading;
using log4net;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;
		private int tickTries;

		public EtlService(EtlJobStarter etlJobStarter)
		{
			tickTries = 0;
			_etlJobStarter = etlJobStarter;
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
				stop = !_etlJobStarter.Tick();
				tickTries = 0;
			}
			catch (Exception ex)
			{
				tickTries++;
				if (ex is SqlException && tickTries <= 3)
				{
					Console.WriteLine(ex);
					log.Warn("Exception occurred invoking EtlJobStarter", ex);
				}
				else
				{
					Console.WriteLine(ex);
					log.Error("Exception occurred invoking EtlJobStarter", ex);
				}
			}

			log.DebugFormat("stop: {0}", stop);
			if (!stop)
			{
				log.Debug("Starting timer");
				_timer?.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
				log.Debug("Timer started");
			}
		}

		public void Dispose()
		{
			log.Info("The service is stopping.");

			_timer?.Dispose();
			_timer = null;
		}
	}
}