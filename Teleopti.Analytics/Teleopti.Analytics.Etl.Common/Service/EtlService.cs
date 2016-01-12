using System;
using System.Timers;
using Autofac;
using log4net;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private Timer _timer;
		private readonly EtlJobStarter _etlJobStarter;

		public EtlService(EtlJobStarter etlJobStarter)
		{
			_etlJobStarter = etlJobStarter;
		}

		public void Start(IContainer container, DateTime serviceStartTime, Action stopService)
		{
			_etlJobStarter.Initialize(container, serviceStartTime, stopService);
			_timer = new Timer(10000);
			_timer.Elapsed += tick;
			_timer.Start();
		}

		void tick(object sender, ElapsedEventArgs e)
		{
			var isStopping = false;
			try
			{
				log.Debug("Tick");
				_timer.Stop();
				log.Debug("Timer stopped");

				var success = _etlJobStarter.Tick();

				isStopping = !success;
			}
			catch (Exception ex)
			{
				log.Error("Exception occurred in tick", ex);
			}
			finally
			{
				try
				{
					log.DebugFormat("Stopping: {0}", isStopping);
					if (!isStopping)
					{
						log.Debug("Starting timer");
						_timer.Start();
						log.Debug("Timer started");
					}
				}
				catch (Exception ex)
				{
					log.Error(ex);
					throw;
				}
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