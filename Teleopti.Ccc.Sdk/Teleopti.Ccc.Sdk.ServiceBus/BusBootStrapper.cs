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
		public BusBootStrapper(IContainer container) : base(container)
		{
		}

	    protected override void ConfigureBusFacility(Rhino.ServiceBus.Impl.AbstractRhinoServiceBusConfiguration configuration)
		{
			base.ConfigureBusFacility(configuration);

			var build = new ContainerBuilder();
			build.RegisterType<DataSourceForTenantWrapper>().SingleInstance();
			build.RegisterType<ApplicationLogOnMessageModule>().As<IMessageModule>().Named<IMessageModule>(typeof(ApplicationLogOnMessageModule).FullName);
			build.Update(Container);
		}

	    protected override bool IsTypeAcceptableForThisBootStrapper(Type t)
        {
        	return true;
        }		
    }

	public class DataSourceForTenantWrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BusBootStrapper));
		private static Lazy<IDataSourceForTenant> _lazy;

		public DataSourceForTenantWrapper(
			Func<IDataSourceForTenant> dataSourceForTenant, 
			ITenantUnitOfWork tenantUnitOfWork,
			IInitializeApplication initializeApplication, 
			ILoadAllTenants loadAllTenants)
		{
			if (_lazy == null)
			{
				_lazy = new Lazy<IDataSourceForTenant>(() =>
				{
					var state = new State();
					var appSettings = ConfigurationManager.AppSettings.ToDictionary();
					initializeApplication.Start(state, null, ConfigurationManager.AppSettings.ToDictionary());
					new InitializeMessageBroker(state.ApplicationScopeData.Messaging).Start(appSettings);

					//////////TODO: Remove this code when bus no longer has to "loop" tenants/datasources later (DataSourceForTenant.DoOnAllTenants_AvoidUsingThis)/////
					using (tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
					{

						loadAllTenants.Tenants().ForEach(dsConf => dataSourceForTenant().MakeSureDataSourceCreated(dsConf.Name,
							dsConf.DataSourceConfiguration.ApplicationConnectionString,
							dsConf.DataSourceConfiguration.AnalyticsConnectionString,
							dsConf.DataSourceConfiguration.ApplicationNHibernateConfig));

					}
					//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

					Logger.Info("Initialized application");
					return dataSourceForTenant();
				});
			}
		}

		public Func<IDataSourceForTenant> DataSource()
		{
			return ()=>_lazy.Value;
		}
	}
}