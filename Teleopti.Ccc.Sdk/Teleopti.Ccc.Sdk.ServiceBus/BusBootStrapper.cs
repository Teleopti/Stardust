using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using Teleopti.Interfaces.Messages.Rta;

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
			configurationReader.ReadConfiguration(new MessageSenderCreator(new InternalServiceBusSender(() => Container.Resolve<IServiceBus>())));
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
		    build.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			build.RegisterModule(SchedulePersistModule.ForOtherModules());
			build.RegisterModule(new ToggleNetModule(ConfigurationManager.AppSettings["FeatureToggle"], ConfigurationManager.AppSettings["ToggleMode"]));

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
}
