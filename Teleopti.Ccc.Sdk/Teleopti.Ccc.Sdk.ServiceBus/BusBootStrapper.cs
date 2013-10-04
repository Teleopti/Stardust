using System;
using Autofac;
using Autofac.Core;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper", Justification = "As the base class is named as it is, this will remain like this."), 
     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper", Justification = "As the base class is named as it is, this will remain like this.")]
    public class BusBootStrapper : AutofacBootStrapper
    {
    	public BusBootStrapper()
    	{
    		var reader = new ConfigurationReaderFactory();
    		var configurationReader = reader.Reader();
			configurationReader.ReadConfiguration(new MessageSenderCreator(new InternalServiceBusSender(()=>Container.Resolve<IServiceBus>(),()=>Container.Resolve<ICurrentIdentity>())));
    	}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	    protected override void ConfigureContainer()
	    {
		    base.ConfigureContainer();

		    var build = new ContainerBuilder();
		    build.RegisterGeneric(typeof (InMemorySagaPersister<>)).As(typeof (ISagaPersister<>));

		    build.RegisterModule<ShiftTradeModule>();
		    build.RegisterModule<RepositoryModule>();
		    build.RegisterModule<AuthorizationContainerInstaller>();
		    build.RegisterModule<AuthenticationContainerInstaller>();
		    build.RegisterModule<AuthenticationModule>();
		    build.RegisterModule<DateAndTimeModule>();
		    build.RegisterModule<SerializationContainerInstaller>();
		    build.RegisterModule<ApplicationInfrastructureContainerInstaller>();
		    build.RegisterModule<PayrollContainerInstaller>();
		    build.RegisterModule<RequestContainerInstaller>();
		    build.RegisterModule<SchedulingContainerInstaller>();
		    build.RegisterModule<ExportForecastContainerInstaller>();
		    build.RegisterModule<ImportForecastContainerInstaller>();
		    build.RegisterModule<ForecastContainerInstaller>();
		    build.RegisterModule<CommandDispatcherModule>();
		    build.RegisterModule<LocalServiceBusEventsPublisherModule>();
		    build.RegisterModule<LocalServiceBusPublisherModule>();
		    build.RegisterModule<CommandHandlersModule>();
		    build.RegisterModule<EventHandlersModule>();
		    build.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>();
		    build.RegisterType<DoNotifySmsLink>().As<IDoNotifySmsLink>();

		    build.Update(Container);
	    }

	    protected override void ConfigureBusFacility(Rhino.ServiceBus.Impl.AbstractRhinoServiceBusConfiguration configuration)
		{
			base.ConfigureBusFacility(configuration);

			var build = new ContainerBuilder();
			build.RegisterType<RaptorDomainMessageModule>().As<IMessageModule>().Named<IMessageModule>(typeof(RaptorDomainMessageModule).FullName);
			build.Update(Container);
		}

        protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }
    }

	public class LocalServiceBusPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LocalServiceBusPublisher>()
			       .As<IPublishEventsFromEventHandlers>()
			       .As<ISendDelayedMessages>()
			       .SingleInstance();
			builder.RegisterType<GetUpdatedScheduleChangeFromTeleoptiRtaService>()
			       .As<IGetUpdatedScheduleChangeFromTeleoptiRtaService>()
			       .SingleInstance();
		}
	}

	public class GetUpdatedScheduleChangeFromTeleoptiRtaService : IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		private readonly IChannelCreator _channelCreator;

		public GetUpdatedScheduleChangeFromTeleoptiRtaService(IChannelCreator channelCreator)
		{
			_channelCreator = channelCreator;
		}

		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			var channel = _channelCreator.CreateChannel<ITeleoptiRtaService>();
			channel.GetUpdatedScheduleChange(personId, businessUnitId, timestamp);
		}
	}

}
