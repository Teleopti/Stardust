using System;
using Autofac;
using Rhino.ServiceBus;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;
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

		    using (Container.Resolve<ITenantUnitOfWork>().Start())
		    {
					var fileConfigurationReader = new FileConfigurationReader(Container.Resolve<IReadDataSourceConfiguration>());
			    fileConfigurationReader.ReadConfiguration(
					new MessageSenderCreator(new InternalServiceBusSender(() => Container.Resolve<IServiceBus>()),
						Container.Resolve<IToggleManager>(), 
						Container.Resolve<Interfaces.MessageBroker.Client.IMessageSender>(),
						Container.Resolve<IJsonSerializer>(),
						Container.Resolve<ICurrentInitiatorIdentifier>()),
				    () => Container.Resolve<IMessageBrokerComposite>());
		    }
		}

        protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }		
    }
}