using System;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server
{
	public class RealTimeContainerInstaller : Module
	{
		private readonly CacheBuilder _cacheBuilder;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public RealTimeContainerInstaller(MbCacheModule mbCacheModule)
		{
			_cacheBuilder = mbCacheModule.Builder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DatabaseConnectionStringHandler>().As<IDatabaseConnectionStringHandler>();
			builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
			builder.RegisterType<RtaBatchHandler>().As<IRtaBatchHandler>();
			//mark activityalarms and stategroups to be cached
			_cacheBuilder
				.For<ActualAgentDataHandler>()
					.CacheMethod(svc => svc.ActivityAlarms())
					.CacheMethod(svc => svc.StateGroups())
					.CacheMethod(svc => svc.GetReadModel(Guid.NewGuid()))
				.As<IActualAgentDataHandler>();

			builder.Register(c =>
									{
										var mbcache = c.Resolve<IMbCacheFactory>();
										var connStringHandler = c.Resolve<IDatabaseConnectionStringHandler>();
										var connFac = c.Resolve<IDatabaseConnectionFactory>();							
										var instance = mbcache.Create<IActualAgentDataHandler>(connFac, connStringHandler);
										return instance;
									});
			builder.RegisterType<ActualAgentHandler>().As<IActualAgentHandler>();
			builder.RegisterType<RtaDataHandler>().As<IRtaDataHandler>();
		}
	}
}