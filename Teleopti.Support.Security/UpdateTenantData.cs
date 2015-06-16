using log4net;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Support.Security
{
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
			//todo: tenant what should we do when/if multiple tenants?
			log.Debug("Updating tenant password...");
			using (_tenantUnitOfWorkManager.Start())
			{
				var updatePasswords = new RegenerateAllTenantPasswords(_tenantUnitOfWorkManager);
				updatePasswords.Modify();
			}
			log.Debug("Updating tenant password. Done!");
		}
	}
}