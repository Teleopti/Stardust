﻿using System;
using Autofac;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Autofac;
using Rhino.ServiceBus.MessageModules;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusBootStrapper : AutofacBootStrapper
    {
    	public BusBootStrapper(IContainer container) : base(container)
    	{
    		var reader = new ConfigurationReaderFactory();
    		var configurationReader = reader.Reader();
			configurationReader.ReadConfiguration(new MessageSenderCreator(new InternalServiceBusSender(() => Container.Resolve<IServiceBus>())));
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
