using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class TenantExists : ITenantExists
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public TenantExists(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public bool Check(string tenantName)
		{
			return _currentTenantSession.CurrentSession()
				.CreateQuery("select count(id) from Tenant t where t.Name=:name")
				.SetString("name", tenantName)
				.UniqueResult<long>() == 1;
		}
	}
}