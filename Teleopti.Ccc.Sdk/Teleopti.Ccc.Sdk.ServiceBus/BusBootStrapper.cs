using System;
using System.Configuration;
using Autofac;
using log4net;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusBootStrapper : AutofacBootStrapper
    {
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BusBootStrapper));

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
			    if (StateHolderReader.IsInitialized)
			    {
				    Logger.Info("StateHolder already initialized. This step is skipped.");
				    return;
			    }
			    var application = Container.Resolve<IInitializeApplication>();
			    using (Container.Resolve<ITenantUnitOfWork>().Start())
			    {
				    application.Start(new BasicState(), null, ConfigurationManager.AppSettings.ToDictionary(), true);

				    Logger.Info("Initialized application");
			    }
		    }
		}
		    

		    protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }		
    }
}