using System;
using System.Configuration;
using System.Linq;
using Autofac;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Sdk.ServiceBus.ShiftTrade;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper", Justification = "As the base class is named as it is, this will remain like this."), 
     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper", Justification = "As the base class is named as it is, this will remain like this.")]
    public class BusBootStrapper : AutofacBootStrapper
    {
    	public BusBootStrapper()
    	{
			SdkConfigurationReader sdkConfigurationReader = new SdkConfigurationReader();
			sdkConfigurationReader.ReadConfiguration(daBus);
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "da")]
		protected IServiceBus daBus()
		{
			return Container.Resolve<IServiceBus>();
		}

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

        	var build = new ContainerBuilder();
        	build.RegisterGeneric(typeof (InMemorySagaPersister<>)).As(typeof (ISagaPersister<>));
            
            bool enableErrorNotifier;

            if (bool.TryParse(ConfigurationManager.AppSettings["EnableErrorNotifier"],out enableErrorNotifier) && enableErrorNotifier)
                build.RegisterType<ErrorNotification>();

            build.RegisterModule<ShiftTradeContainerInstaller>();
        	build.RegisterModule<RepositoryContainerInstaller>();
        	build.RegisterModule<AuthorizationContainerInstaller>();
        	build.RegisterModule<AuthenticationContainerInstaller>();
        	build.RegisterModule<SerializationContainerInstaller>();
        	build.RegisterModule<ApplicationInfrastructureContainerInstaller>();
        	build.RegisterModule<PayrollContainerInstaller>();
        	build.RegisterModule<RequestContainerInstaller>();
        	build.RegisterModule<SchedulingContainerInstaller>();
        	build.RegisterModule<ExportForecastContainerInstaller>();
        	build.RegisterModule<ImportForecastContainerInstaller>();
			build.RegisterModule<ForecastContainerInstaller>();

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
