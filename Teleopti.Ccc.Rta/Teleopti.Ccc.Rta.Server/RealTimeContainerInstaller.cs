﻿using System;
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
			//mark activityalarms and stategroups to be cached
			_cacheBuilder
				.For<DatabaseHandler>()
					.CacheMethod(svc => svc.ActivityAlarms())
					.CacheMethod(svc => svc.StateGroups())
					.CacheMethod(svc => svc.GetReadModel(Guid.NewGuid()))
				.As<IDatabaseHandler>();

			builder.Register(c =>
									{
										var mbcache = c.Resolve<IMbCacheFactory>();
										var connStringHandler = c.Resolve<IDatabaseConnectionStringHandler>();
										var connFac = c.Resolve<IDatabaseConnectionFactory>();							
										var instance = mbcache.Create<IDatabaseHandler>(connFac, connStringHandler);
										return instance;
									});
			builder.RegisterType<ActualAgentAssembler>().As<IActualAgentAssembler>();
			builder.RegisterType<RtaDataHandler>().As<IRtaDataHandler>();
			builder.RegisterType<ActualAgentStateCache>().As<IActualAgentStateCache>().SingleInstance();
			builder.RegisterType<AlarmMapper>().As<IAlarmMapper>();
		}
	}
}