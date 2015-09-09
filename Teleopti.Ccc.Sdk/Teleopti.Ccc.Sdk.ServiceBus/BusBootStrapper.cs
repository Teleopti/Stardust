using System;
using System.Configuration;
using Autofac;
using log4net;
using Rhino.ServiceBus.MessageModules;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
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

	    private static IDataSourceForTenant dataSourceForTenant;

	    protected override void ConfigureBusFacility(Rhino.ServiceBus.Impl.AbstractRhinoServiceBusConfiguration configuration)
		{
			base.ConfigureBusFacility(configuration);

			var build = new ContainerBuilder();
			build.RegisterType<ApplicationLogOnMessageModule>().As<IMessageModule>().Named<IMessageModule>(typeof(ApplicationLogOnMessageModule).FullName);
			build.Update(Container);

		  if (dataSourceForTenant == null)
		  {
			  initApplicationAndSetDataSourceForTenant();
		  }
		  else
		  {
			  reuseAlreadyCreatedDataSourceForTenant();
		  }
		}

	    private void reuseAlreadyCreatedDataSourceForTenant()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterInstance(dataSourceForTenant).As<IDataSourceForTenant>().SingleInstance();
		    builder.Update(Container);
	    }

	    private void initApplicationAndSetDataSourceForTenant()
	    {
			var dsForTenant = Container.Resolve<IDataSourceForTenant>();
			using (Container.Resolve<ITenantUnitOfWork>().Start())
		    {
			    var application = Container.Resolve<IInitializeApplication>();
			    using (Container.Resolve<ITenantUnitOfWork>().Start())
			    {
				    var loadAllTenants = Container.Resolve<ILoadAllTenants>();
						application.Start(new BasicState(), null, ConfigurationManager.AppSettings.ToDictionary(), true);
					loadAllTenants.Tenants().ForEach(dsConf =>
					{
						dsForTenant.MakeSureDataSourceExists(dsConf.Name,
							dsConf.DataSourceConfiguration.ApplicationConnectionString,
							dsConf.DataSourceConfiguration.AnalyticsConnectionString,
							dsConf.DataSourceConfiguration.ApplicationNHibernateConfig);
					});


					Logger.Info("Initialized application");
			    }
			    dataSourceForTenant = dsForTenant;
		    }
	    }


	    protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }		
    }
}