using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;
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

			#region All implementation of IPersistCallback
			var businessUnit = CurrentBusinessUnit.Make();

			/**************************************/
			/*         Order dependant            */
			builder.RegisterType<EventsMessageSender>().As<IPersistCallback>();
			if (_configuration.Args().OptimizeScheduleChangedEvents_DontUseFromWeb)
				builder.RegisterType<ScheduleChangedEventPublisher>().As<IPersistCallback>();
			/*           End                      */
			/**************************************/

			builder.RegisterType<ScheduleChangedEventFromMeetingPublisher>().As<IPersistCallback>();
			builder.RegisterType<GroupPageChangedBusMessageSender>().As<IPersistCallback>();
			builder.Register(
				c => new PersonCollectionChangedEventPublisherForTeamOrSite(c.Resolve<IEventPopulatingPublisher>(), businessUnit))
				.As<IPersistCallback>();
			builder.Register(c => new PersonCollectionChangedEventPublisher(c.Resolve<IEventPopulatingPublisher>(), businessUnit))
				.As<IPersistCallback>();
			builder.RegisterType<PersonPeriodChangedBusMessagePublisher>().As<IPersistCallback>();

			if (_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
			{
				builder.Register(c => new ScheduleChangedMessageSender(c.Resolve<IMessageSender>(), CurrentDataSource.Make(),
					businessUnit, c.Resolve<IJsonSerializer>(), c.Resolve<ICurrentInitiatorIdentifier>())).As<IPersistCallback>();
			}

			builder.RegisterType<CurrentPersistCallbacks>().As<ICurrentPersistCallbacks>().As<IMessageSendersScope>();
			#endregion

			builder.RegisterType<CurrentBusinessUnit>().As<ICurrentBusinessUnit>().SingleInstance()
				.OnActivated(e => ServiceLocatorForEntity.SetInstanceFromContainer(e.Instance))
				.OnRelease(e => ServiceLocatorForEntity.SetInstanceFromContainer(null as ICurrentBusinessUnit));
			builder.RegisterType<HttpRequestFalse>().As<IIsHttpRequest>().SingleInstance();

			builder.RegisterType<CurrentInitiatorIdentifier>().As<ICurrentInitiatorIdentifier>();
			builder.RegisterType<BusinessUnitFilterOverrider>().As<IBusinessUnitFilterOverrider>().SingleInstance();
			builder.RegisterType<DisableBusinessUnitFilter>().As<IDisableBusinessUnitFilter>().SingleInstance();

			// these keep scope state and cant be single instance
			builder.RegisterType<UnitOfWorkAspect>().As<IUnitOfWorkAspect>().InstancePerDependency();
			builder.RegisterType<ReadOnlyUnitOfWorkAspect>().As<IReadOnlyUnitOfWorkAspect>().InstancePerDependency();
			builder.RegisterType<AllBusinessUnitsUnitOfWorkAspect>().As<IAllBusinessUnitsUnitOfWorkAspect>().InstancePerDependency();
		}
	}
}
