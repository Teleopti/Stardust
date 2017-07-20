using System;
using System.Configuration;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	// This is needed for payroll so don't remove this completely when (if) we remove rhino
	public class DataSourceForTenantWrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(DataSourceForTenantWrapper));
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
					initializeApplication.Start(state, null, appSettings);
					new InitializeMessageBroker(initializeApplication.Messaging).Start(appSettings);

					//////////TODO: Remove this code when bus no longer has to "loop" tenants/datasources later (DataSourceForTenant.DoOnAllTenants_AvoidUsingThis)/////
					using (tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
					{

						loadAllTenants.Tenants().ForEach(dsConf => dataSourceForTenant().MakeSureDataSourceCreated(dsConf.Name,
							dsConf.DataSourceConfiguration.ApplicationConnectionString,
							dsConf.DataSourceConfiguration.AnalyticsConnectionString,
							dsConf.ApplicationConfig));

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