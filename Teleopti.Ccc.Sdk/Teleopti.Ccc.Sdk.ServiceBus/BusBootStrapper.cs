using System;
using Autofac;
using Rhino.ServiceBus;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusBootStrapper : AutofacBootStrapper
    {
    	public BusBootStrapper(IContainer container) : base(container)
		{
		}

	    protected override void ConfigureBusFacility(Rhino.ServiceBus.Impl.AbstractRhinoServiceBusConfiguration configuration)
		{
			base.ConfigureBusFacility(configuration);

			var build = new ContainerBuilder();
			build.RegisterType<ApplicationLogOnMessageModule>().As<IMessageModule>().Named<IMessageModule>(typeof(ApplicationLogOnMessageModule).FullName);
			build.Update(Container);

			var reader = new ConfigurationReaderFactory();
			var configurationReader = reader.Reader();
			configurationReader.ReadConfiguration(new MessageSenderCreator(new InternalServiceBusSender(() => Container.Resolve<IServiceBus>())), () => Container.Resolve<IMessageBrokerComposite>());
		}

        protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }		
    }
}