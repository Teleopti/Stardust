using System.Linq;
using log4net;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Support.Security
{
	//todo: tenant what should we do when/if multiple tenants?
	public class UpdateTenantData
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UpdateTenantData));
		private readonly TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public UpdateTenantData(TenantUnitOfWorkManager tenantUnitOfWorkManager)
		{
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public void RegenerateTenantPasswords()
		{
			log.Debug("Skip updating passwords due to bug #33838. Do this is some other way instead?");
		}

		public void UpdateTenantConnectionStrings(string appDbConnectionString, string analyticsDbConnectionString)
		{
			log.Debug("Updating tenant connection strings...");
			using (_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted())
			{
				var allTenants = new LoadAllTenants(_tenantUnitOfWorkManager).Tenants();
				var numberOfTenants = allTenants.Count();
				if (numberOfTenants != 1)
				{
					log.DebugFormat("Skipping updating tenant connection strings because there are {0} tenants in db.", numberOfTenants);
					return;
				}
				var tenant = allTenants.Single();
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString(analyticsDbConnectionString);
				tenant.DataSourceConfiguration.SetApplicationConnectionString(appDbConnectionString);
			}
			log.Debug("Updating tenant connection strings. Done!");
		}
	}
}