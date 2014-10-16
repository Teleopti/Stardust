using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaModule : Module
	{
		private readonly IIocConfiguration _config;

		public RtaModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DatabaseConnectionStringHandler>().As<IDatabaseConnectionStringHandler>();
			builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
			//mark activityalarms and stategroups to be cached
			_config.Args().CacheBuilder
				.For<DatabaseReader>()
				.CacheMethod(x => x.ActivityAlarms())
				.CacheMethod(x => x.StateGroups())
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.Register<IGetCurrentActualAgentState>(c => c.Resolve<DatabaseReader>());
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();
			builder.RegisterType<ActualAgentAssembler>().As<IActualAgentAssembler>();
			builder.RegisterType<RtaDataHandler>().As<IRtaDataHandler>();
			builder.RegisterType<AlarmMapper>().As<IAlarmMapper>();

			builder.RegisterType<CurrentAndNextLayerExtractor>().As<ICurrentAndNextLayerExtractor>().SingleInstance();
			builder.RegisterType<DataSourceResolver>().As<IDataSourceResolver>();
			builder.RegisterType<PersonResolver>().As<IPersonResolver>();
			builder.RegisterType<AdherenceAggregatorInitializor>().AsSelf().As<IAdherenceAggregatorInitializor>();

			registerAdherenceComponents(builder);
		}

		private void registerAdherenceComponents(ContainerBuilder builder)
		{
			builder.RegisterType<AdherenceAggregator>().SingleInstance().As<IActualAgentStateHasBeenSent>();
			builder.RegisterType<AdherencePercentageReadModelPersister>().SingleInstance().As<IAdherencePercentageReadModelPersister>();
			if (_config.Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783))
				builder.RegisterType<AgentStateChangedCommandHandler>().SingleInstance().As<IActualAgentStateHasBeenSent>();

			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			_config.Args().CacheBuilder
				.For<PersonOrganizationProvider>()
				.CacheMethod(svc => svc.PersonOrganizationData())
				.As<IPersonOrganizationProvider>();
			builder.RegisterMbCacheComponent<PersonOrganizationProvider, IPersonOrganizationProvider>().SingleInstance();
				
			//messy for now
			builder.Register(c => new PersonOrganizationReader(c.Resolve<INow>(), c.Resolve<IDatabaseConnectionStringHandler>().AppConnectionString()))
				.SingleInstance().As<IPersonOrganizationReader>();
		}
	}
}