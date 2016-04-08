using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

			builder.RegisterType<WithUnitOfWork>().SingleInstance();

			persistCallbacks(builder);

			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>().SingleInstance();
			builder.RegisterType<NoBusinessUnitForRequest>().As<IBusinessUnitForRequest>().SingleInstance();

			builder.RegisterType<CurrentInitiatorIdentifier>()
				.As<ICurrentInitiatorIdentifier>()
				.As<IInitiatorIdentifierScope>()
				.SingleInstance();
			builder.RegisterType<BusinessUnitFilterOverrider>().As<IBusinessUnitFilterOverrider>().SingleInstance();
			builder.RegisterType<DisableBusinessUnitFilter>().As<IDisableBusinessUnitFilter>().SingleInstance();

			// these keep scope state and cant be single instance
			builder.RegisterType<UnitOfWorkAspect>().As<IUnitOfWorkAspect>().InstancePerDependency();
			builder.RegisterType<ReadOnlyUnitOfWorkAspect>().As<IReadOnlyUnitOfWorkAspect>().InstancePerDependency();
			builder.RegisterType<AllBusinessUnitsUnitOfWorkAspect>().As<IAllBusinessUnitsUnitOfWorkAspect>().InstancePerDependency();
		}

		private void persistCallbacks(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentPersistCallbacks>().As<ICurrentPersistCallbacks>().As<IMessageSendersScope>();

			/**************************************/
			/*         Order dependant            */
			builder.RegisterType<EventsMessageSender>().As<IPersistCallback>();
			if (_configuration.Args().OptimizeScheduleChangedEvents_DontUseFromWeb)
				builder.RegisterType<ScheduleChangedEventPublisher>().As<IPersistCallback>();
			/*           End                      */
			/**************************************/

			builder.RegisterType<ScheduleChangedEventFromMeetingPublisher>().As<IPersistCallback>();
			builder.RegisterType<GroupPageCollectionChangedEventPublisher>().As<IPersistCallback>();
			builder.RegisterType<PersonCollectionChangedEventPublisherForTeamOrSite>().As<IPersistCallback>();
			builder.RegisterType<PersonCollectionChangedEventPublisher>().As<IPersistCallback>();
			builder.RegisterType<SettingsForPersonPeriodChangedEventPublisher>().As<IPersistCallback>();
			builder.RegisterType<MessageBrokerSender>().As<IPersistCallback>().SingleInstance();
			if (_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				builder.RegisterType<ScheduleChangedMessageSender>().As<IPersistCallback>();

		}
	}
}
