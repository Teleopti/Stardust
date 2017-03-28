using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RepositoryModule : Module
	{
		private readonly Type repositoryConstructorType = typeof (ICurrentUnitOfWork);

		protected override void Load(ContainerBuilder builder)
		{
			foreach (var type in typeof(PersonRepository).Assembly.GetTypes().Where(t => isRepository(t) && hasCorrectCtor(t)))
			{
				if (type.GetConstructors().Length == 1)
				{
					builder.RegisterType(type)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
				else if (type.GetConstructor(new[] { repositoryConstructorType }) != null)
				{
					builder.RegisterType(type)
						.UsingConstructor(repositoryConstructorType)
						.AsImplementedInterfaces()
						.SingleInstance();
				}
			}

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
			       .InstancePerDependency();

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
			builder.RegisterType<SmartPersonPropertyQuerier>().As<ISmartPersonPropertyQuerier>().SingleInstance();
		}

		private bool hasCorrectCtor(Type repositoryType)
		{
			var constructors = repositoryType.GetConstructors();
			if (constructors.Length == 1)
				return true;
			foreach (var constructorInfo in constructors)
			{
				var parameters = constructorInfo.GetParameters();
				if (parameters.Length == 1)
				{
					if (parameters[0].ParameterType == repositoryConstructorType)
						return true;
				}
			}
			return false;
		}

		private static bool isRepository(Type infrastructureType)
		{
			return infrastructureType.Name.EndsWith("Repository", StringComparison.Ordinal);
		}
	}
}