using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface ICheckTenantUserExists
	{
		bool Exists();
	}
	public class CheckTenantUserExists : ICheckTenantUserExists
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private bool exists;

		public CheckTenantUserExists(ICurrentTenantSession currentTenantSession, ITenantUnitOfWork tenantUnitOfWork)
		{
			_currentTenantSession = currentTenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public bool Exists()
		{
			if (checkStoredFieldInsteadOfGoingToDatabaseForEachCall()) return true;

			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				exists = _currentTenantSession.CurrentSession()
							.GetNamedQuery("loadNumberOfTenantUsers")
							.UniqueResult<long>() > 0L;
				return exists;
			}
		}

		private bool checkStoredFieldInsteadOfGoingToDatabaseForEachCall()
		{
			return exists;
		}
	}
}