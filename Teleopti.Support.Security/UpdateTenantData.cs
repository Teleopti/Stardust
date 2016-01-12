using System.Linq;
using log4net;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Support.Security
{
	//todo: tenant what should we do when/if multiple tenants?
	public class UpdateTenantData
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentTenantSession _currentTenantSession;
		private static readonly ILog log = LogManager.GetLogger(typeof(UpdateTenantData));

		public UpdateTenantData(
			ITenantUnitOfWork tenantUnitOfWork,
			ICurrentTenantSession currentTenantSession)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_currentTenantSession = currentTenantSession;
		}

		public void RegenerateTenantPasswords()
		{
			log.Debug("Skip updating passwords due to bug #33838. Do this is some other way instead?");
		}

		public void UpdateTenantConnectionStrings(string appDbConnectionString, string analyticsDbConnectionString)
		{
			log.Debug("Updating tenant connection strings...");
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var allTenants = new LoadAllTenants(_currentTenantSession).Tenants();
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