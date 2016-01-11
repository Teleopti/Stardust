using System;
using System.Timers;
using Autofac;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Timer = System.Timers.Timer;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	public class EtlService : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlService));

		private readonly IContainer _container;
		private readonly Timer _timer;
		private EtlJobStarter _etlJobStarter;

		public EtlService()
		{
			XmlConfigurator.Configure();

			try
			{
				_container = configureContainer();
				_etlJobStarter = new EtlJobStarter();
				_timer = new Timer(10000);
				_timer.Elapsed += tick;
			}
			catch (Exception ex)
			{
				log.Error("The service could not be started", ex);
				throw;
			}
		}

		private static IContainer configureContainer()
		{
			var builder = new ContainerBuilder();
			var iocArgs = new IocArgs(new ConfigReader());
			var configuration = new IocConfiguration(
				iocArgs,
				CommonModule.ToggleManagerForIoc(iocArgs)
				);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new EtlModule(configuration));
			return builder.Build();
		}

		public void Start(Action stopService)
		{
			log.Info("The service is starting.");
			var serviceStartTime = DateTime.Now;
			_etlJobStarter.Init(_container, serviceStartTime, stopService);
			_timer.Start();
			log.Info("The service started at " + serviceStartTime);
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
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				log.Info("The service is stopping.");
				if (_timer != null)
				{
					_timer.Stop();
					_timer.Dispose();
				}
				if (_etlJobStarter != null)
				{
					_etlJobStarter.Dispose();
					_etlJobStarter = null;
				}
			}
		}

	}

}