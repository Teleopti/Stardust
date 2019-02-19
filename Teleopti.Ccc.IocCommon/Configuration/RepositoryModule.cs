using System;
using System.Configuration;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Stardust;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MachineLearning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RepositoryModule : Module
	{
		private readonly IocConfiguration _configuration;
		private readonly Type[] constructorFallback1 = {typeof(ICurrentUnitOfWork), typeof(ICurrentBusinessUnit), typeof(Lazy<IUpdatedBy>)};
		private readonly Type[] constructorFallback2 = typeof(ICurrentUnitOfWork).AsArray();

		public RepositoryModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_QueryHintOnLayers_79780)]
		private static void specialPersonAssignmentRegistration(ContainerBuilder builder)
		{
			builder
				.RegisterToggledComponent<PersonAssignmentRepositoryWithQueryHint, PersonAssignmentRepository,
					IPersonAssignmentRepository>(Toggles.ResourcePlanner_QueryHintOnLayers_79780).SingleInstance();
		}

		protected override void Load(ContainerBuilder builder)
		{
			var repositories = typeof(PersonRepository).Assembly.GetExportedTypes()
				.Where(isRepository)
				.Where(hasRepositoryConstructor);
			
			foreach (var type in repositories)
			{
				if (type.GetConstructors().Length == 1)
				{
					builder.RegisterType(type)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
				else if (type.GetConstructor(constructorFallback1) != null)
				{
					builder.RegisterType(type)
						.UsingConstructor(constructorFallback1)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
				else if (type.GetConstructor(constructorFallback2) != null)
				{
					builder.RegisterType(type)
						.UsingConstructor(constructorFallback2)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
			}

			specialPersonAssignmentRegistration(builder);

			builder.RegisterType<PersonLoadAllWithAssociation>().As<IPersonLoadAllWithAssociation>().SingleInstance();

			builder.RegisterType<ScheduleStorageRepositoryWrapper>().As<IScheduleStorageRepositoryWrapper>();
			builder.RegisterType<ProjectionVersionPersister>()
				.As<IProjectionVersionPersister>()
				.SingleInstance();
			builder.RegisterType<PersonAssociationPublisherCheckSumPersister>()
				.As<IPersonAssociationPublisherCheckSumPersister>()
				.SingleInstance();
			builder.RegisterType<PushMessagePersister>()
				.As<IPushMessagePersister>()
				.SingleInstance();
			builder.RegisterType<CreatePushMessageDialoguesService>()
				.As<ICreatePushMessageDialoguesService>()
				.SingleInstance();
			builder.RegisterType<EtlJobStatusRepository>()
				.As<IEtlJobStatusRepository>()
				.SingleInstance();
			builder.RegisterType<EtlLogObjectRepository>()
				.As<IEtlLogObjectRepository>()
				.SingleInstance();

			builder.Register(c => StatisticRepositoryFactory.Create())
				.As<IStatisticRepository>();

			builder.RegisterType<DefaultScenarioFromRepository>()
				.As<ICurrentScenario>()
				.SingleInstance();

			builder.RegisterType<LoadUserUnauthorized>()
				.As<ILoadUserUnauthorized>()
				.SingleInstance();

			builder.RegisterType<AggregateRootInitilizer>()
				.As<IAggregateRootInitializer>()
				.SingleInstance();

			builder.RegisterType<AuditSettingRepository>()
				.As<IAuditSettingRepository>()
				.SingleInstance();

			builder.RegisterType<ReadModelValidationResultPersister>()
				.As<IReadModelValidationResultPersister>()
				.SingleInstance();

			builder.RegisterType<PersonForScheduleFinder>().As<IPersonForScheduleFinder>().SingleInstance();
			builder.RegisterType<PeopleForShiftTradeFinder>().As<IPeopleForShiftTradeFinder>().SingleInstance();
			builder.RegisterType<PersonInRoleQuerier>().As<IPersonInRoleQuerier>().SingleInstance();
			builder.RegisterType<ShiftCategoryUsageFinder>().As<IShiftCategoryUsageFinder>().SingleInstance();
			builder.RegisterType<PredictShiftCategory>().As<IPredictShiftCategory>().SingleInstance();
			builder.RegisterType<ShiftCategoryPredictionModelLoader>().As<IShiftCategoryPredictionModelLoader>().SingleInstance();

			builder.RegisterType<PurgeSettingRepository>().As<IPurgeSettingRepository>().SingleInstance();

			builder.Register(c => new StardustRepository(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)).As<IStardustRepository>().As<IGetAllWorkerNodes>().SingleInstance();
		}

		private bool hasRepositoryConstructor(Type type)
		{
			if (type.GetConstructors().Length == 1)
				return true;
			if (type.GetConstructor(constructorFallback1) != null)
				return true;
			if (type.GetConstructor(constructorFallback2) != null)
				return true;
			return false;
		}

		private static bool isRepository(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("Repository", StringComparison.Ordinal);
		}
	}
}