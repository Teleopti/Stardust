using System;
using Autofac;
using log4net;
using log4net.Config;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlServiceHost : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlServiceHost));
		RetryPolicy _retryPolicy;

		private IContainer _container;

		public void SimplyEnsureRecurringJobs()
		{
			XmlConfigurator.Configure();
			_retryPolicy = new RetryPolicy<retryingOnAllStrategy>(14, TimeSpan.FromSeconds(4));

			_retryPolicy.ExecuteAction(() =>
			{
				var service = buildEtlService();
				service.EnsureSystemWideRecurringJobs();
				service.EnsureTenantRecurringJobs();
			});
		}

		public void Start(Action stopService)
		{
			XmlConfigurator.Configure();
			_retryPolicy = new RetryPolicy<retryingOnAllStrategy>(14, TimeSpan.FromSeconds(4));

			log.Info("The service is starting.");

			var serviceStartTime = DateTime.Now;
			try
			{
				_retryPolicy.ExecuteAction(() =>
				{
					var service = buildEtlService();
					service.Start(serviceStartTime, stopService);
				});
			}
			catch (Exception ex)
			{
				log.Error("The service could not be started", ex);
				throw;
			}

			log.Info("The service started at " + serviceStartTime);
		}

		private EtlService buildEtlService()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new EtlAppModule());
			_container = builder.Build();
			_container.Resolve<HangfireClientStarter>().Start();
			return _container.Resolve<EtlService>();
		}

		public void Dispose()
		{
			_container?.Dispose();
		}

		private class retryingOnAllStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				log.Warn("The service could not be started, will retry to start", ex);
				return true;
			}
		}
	}
}