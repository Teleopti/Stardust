using System;
using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaCommonModule : Module
	{
		private readonly CacheBuilder _cacheBuilder;

		public RtaCommonModule(MbCacheModule mbCacheModule)
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

			builder.RegisterType<CurrentAndNextLayerExtractor>().As<ICurrentAndNextLayerExtractor>().SingleInstance();
			builder.RegisterType<DataSourceResolver>().As<IDataSourceResolver>();
			builder.RegisterType<PersonResolver>().As<IPersonResolver>();
			builder.RegisterType<AdherenceAggregatorInitializor>().AsSelf().As<IAdherenceAggregatorInitializor>();

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