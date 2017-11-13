using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class UnitOfWorkModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public UnitOfWorkModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType(_configuration.Args().ImplementationTypeForCurrentUnitOfWork)
				.As<ICurrentUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
			builder.RegisterType<ThrowOnNestedUnitOfWork>().As<INestedUnitOfWorkStrategy>().SingleInstance();

			builder.RegisterType<WithUnitOfWork>().SingleInstance().ApplyAspects();

			persistCallbacks(builder);

			builder.RegisterType<CurrentBusinessUnit>()
				.As<ICurrentBusinessUnit>()
				.As<IBusinessUnitScope>()
				.SingleInstance();
			builder.RegisterType<BusinessUnitIdIdForRequest>().As<IBusinessUnitIdForRequest>().SingleInstance();

			builder.RegisterType<CurrentInitiatorIdentifier>()
				.As<ICurrentInitiatorIdentifier>()
				.As<IInitiatorIdentifierScope>()
				.SingleInstance();
			builder.RegisterType<DisableBusinessUnitFilter>().As<IDisableBusinessUnitFilter>().SingleInstance();
			builder.RegisterType<DatabaseVersion>().AsSelf().SingleInstance();
			builder.RegisterType<AllBusinessUnitsUnitOfWorkAspect>().As<IAspect>().As<IAllBusinessUnitsUnitOfWorkAspect>().SingleInstance();
			builder.RegisterType<UnitOfWorkAspect>().As<IAspect>().As<IUnitOfWorkAspect>().InstancePerDependency();

			builder.RegisterType<ConnectionStrings>().As<IConnectionStrings>();
		}

		private void persistCallbacks(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentTransactionHooks>().As<ICurrentTransactionHooks>().As<ITransactionHooksScope>();
			builder.RegisterType<CurrentPreCommitHooks>().As<ICurrentPreCommitHooks>();
			builder.RegisterType<SetSourceOnPersonAssignment>().As<IPreCommitHook>();
			builder.RegisterType<CurrentSchedulingSource>().As<ICurrentSchedulingSource>().As<ISchedulingSourceScope>().SingleInstance();

			/**************************************/
			/*         Order dependant            */
			builder.RegisterType<EventsMessageSender>().As<ITransactionHook>();
			if (_configuration.Args().OptimizeScheduleChangedEvents_DontUseFromWeb)
				builder.RegisterType<ScheduleChangedEventPublisher>().As<ITransactionHook>();
			/*           End                      */
			/**************************************/

			builder.RegisterType<ScheduleChangedEventFromMeetingPublisher>().As<ITransactionHook>();
			builder.RegisterType<GroupPageCollectionChangedEventPublisher>().As<ITransactionHook>();
			builder.RegisterType<PersonCollectionChangedEventPublisherForTeamOrSite>().As<ITransactionHook>();
			builder.RegisterType<PersonCollectionChangedEventPublisher>().As<ITransactionHook>();
			builder.RegisterType<OptionalColumnCollectionChangedEventPublisher>().As<ITransactionHook>();
			builder.RegisterType<SettingsForPersonPeriodChangedEventPublisher>().As<ITransactionHook>();
			builder.RegisterType<MessageBrokerSender>().As<ITransactionHook>().SingleInstance();
			builder.RegisterType<ScheduleChangedMessageSender>().As<ITransactionHook>();
		}
	}
}