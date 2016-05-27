using log4net;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Support.Security
{
	//This is just used to make sure that one tenant points to same server and database that the information is stored
	//this is to prevent wrong use when restoring a copy to test a patching, and end up patching the original
	public class CheckTenantConnectionStrings
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentTenantSession _currentTenantSession;
		private static readonly ILog log = LogManager.GetLogger(typeof(UpdateTenantData));

		public CheckTenantConnectionStrings(
			ITenantUnitOfWork tenantUnitOfWork,
			ICurrentTenantSession currentTenantSession)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_currentTenantSession = currentTenantSession;
		}

		public void CheckConnectionStrings()
		{
			log.Debug("Checking tenant connection strings...");
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				//execute some function in infrastructure
			}
			log.Debug("Checking tenant connection strings. Done!");
		}
	}
}