using System;
using System.Configuration;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.SignalR;
using Module = Autofac.Module;

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
				.CacheMethod(x => x.ActivityAlarms())
				.CacheMethod(x => x.StateGroups())
				.CacheMethod(x => x.GetReadModel(Guid.NewGuid()))
				.CacheMethod(x => x.LoadDatasources())
				.CacheMethod(x => x.LoadAllExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.Register<ILoadActualAgentState>(c => c.Resolve<DatabaseReader>());
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();
			builder.RegisterType<ActualAgentAssembler>().As<IActualAgentAssembler>();
			builder.RegisterType<RtaDataHandler>().As<IRtaDataHandler>();
			builder.RegisterType<AlarmMapper>().As<IAlarmMapper>();
			builder.Register(c => new RecreateOnNoPingReply(TimeSpan.FromMinutes(1))).As<IConnectionKeepAliveStrategy>();
			builder.Register(c => new RestartOnClosed(TimeSpan.Zero)).As<IConnectionKeepAliveStrategy>();
			builder.RegisterType<SignalRClient>()
				.As<ISignalRClient>()
				.WithParameter(new NamedParameter("serverUrl", ConfigurationManager.AppSettings["MessageBroker"]))
				.SingleInstance();
			builder.RegisterType<SignalRSender>()
				.As<IMessageSender>()
				.SingleInstance();
			builder.RegisterType<CurrentAndNextLayerExtractor>().As<ICurrentAndNextLayerExtractor>().SingleInstance();
			builder.RegisterType<DataSourceResolver>().As<IDataSourceResolver>();
			builder.RegisterType<PersonResolver>().As<IPersonResolver>();
			builder.RegisterType<AdherenceAggregatorInitializor>().AsSelf().As<IAdherenceAggregatorInitializor>();
			builder.RegisterModule<DateAndTimeModule>();

			registerAdherenceComponents(builder);
		}

		private void registerAdherenceComponents(ContainerBuilder builder)
		{
			builder.RegisterType<AdherenceAggregator>().SingleInstance().As<IActualAgentStateHasBeenSent>();
			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			_cacheBuilder
				.For<PersonOrganizationProvider>()
				.CacheMethod(svc => svc.LoadAll())
				.As<IPersonOrganizationProvider>();
			builder.RegisterMbCacheComponent<PersonOrganizationProvider, IPersonOrganizationProvider>().SingleInstance();
				
			//messy for now
			builder.Register(c => new PersonOrganizationReader(c.Resolve<INow>(), c.Resolve<IDatabaseConnectionStringHandler>().AppConnectionString()))
				.SingleInstance().As<IPersonOrganizationReader>();
		}
	}
}