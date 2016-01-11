using System;
using Autofac;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlServiceHost : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlServiceHost));

		private IContainer _container;
		
		public void Start(Action stopService)
		{
			XmlConfigurator.Configure();

			log.Info("The service is starting.");

			var serviceStartTime = DateTime.Now;
			try
			{
				var builder = new ContainerBuilder();
				var iocArgs = new IocArgs(new ConfigReader());
				var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
				builder.RegisterModule(new CommonModule(configuration));
				builder.RegisterModule(new EtlModule(configuration));
				_container = builder.Build();

				var service = _container.Resolve<EtlService>();
				service.Start(_container, serviceStartTime, stopService);
			}
			catch (Exception ex)
			{
				log.Error("The service could not be started", ex);
				throw;
			}

			log.Info("The service started at " + serviceStartTime);
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
				if (_container != null)
					_container.Dispose();
			}
		}

	}

}