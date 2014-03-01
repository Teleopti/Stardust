using System;
using System.Configuration;
using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server
{
	public class RealTimeContainerInstaller : Module
	{
		private readonly CacheBuilder _cacheBuilder;

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
				.For<DatabaseReader>()
					.CacheMethod(svc => svc.ActivityAlarms())
					.CacheMethod(svc => svc.StateGroups())
					.CacheMethod(svc => svc.GetReadModel(Guid.NewGuid()))
				.As<IDatabaseReader>();

			builder.RegisterType<DatabaseReader>().AsSelf();
			builder.Register<IDatabaseReader>(c => c.Resolve<DatabaseReader>())
				.IntegrateWithMbCache();

			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();
			builder.RegisterType<ActualAgentAssembler>().As<IActualAgentAssembler>();
			builder.RegisterType<RtaDataHandler>().As<IRtaDataHandler>();
			builder.RegisterType<ActualAgentStateCache>().As<IActualAgentStateCache>().SingleInstance();
			builder.RegisterType<AlarmMapper>().As<IAlarmMapper>();
			builder.RegisterType<AsyncSignalSender>().As<IAsyncMessageSender>().WithParameter(new NamedParameter("serverUrl", ConfigurationManager.AppSettings["MessageBroker"])).SingleInstance();
			builder.RegisterType<CurrentAndNextLayerExtractor>().As<ICurrentAndNextLayerExtractor>().SingleInstance();
			builder.RegisterType<DataSourceResolver>().As<IDataSourceResolver>();
			builder.RegisterType<PersonResolver>().As<IPersonResolver>();
		}
	}
}