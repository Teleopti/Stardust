using System;
using System.Configuration;
using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;
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

		protected override void Load(Autofac.ContainerBuilder builder)
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
			builder.RegisterType<SignalSender>()
				.As<IMessageSender>()
				.WithParameter(new NamedParameter("serverUrl", ConfigurationManager.AppSettings["MessageBroker"]))
				.SingleInstance();
			builder.RegisterType<CurrentAndNextLayerExtractor>().As<ICurrentAndNextLayerExtractor>().SingleInstance();
			builder.RegisterType<DataSourceResolver>().As<IDataSourceResolver>();
			builder.RegisterType<PersonResolver>().As<IPersonResolver>();

			builder.RegisterModule<DateAndTimeModule>();

			registerAdherenceComponents(builder);
		}

		private static void registerAdherenceComponents(Autofac.ContainerBuilder builder)
		{
			builder.RegisterType<AdherenceAggregator>().SingleInstance().As<IActualAgentStateHasBeenSent>();
			builder.RegisterType<TeamIdForPerson>().SingleInstance().As<ITeamIdForPerson>();
			builder.RegisterType<SiteIdForPerson>().SingleInstance().As<ISiteIdForPerson>();
			builder.RegisterType<PersonOrganizationProvider>().SingleInstance().As<IPersonOrganizationProvider>();
			//messy for now
			builder.Register(c => new PersonOrganizationReader(c.Resolve<INow>(), c.Resolve<IDatabaseConnectionStringHandler>().AppConnectionString()))
				.SingleInstance().As<IPersonOrganizationReader>();
		}
	}
}