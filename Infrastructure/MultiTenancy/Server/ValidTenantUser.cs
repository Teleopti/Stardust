using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ValidTenantUser : IValidTenantUser
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ValidTenantUser(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public Tenant IsValidForTenant(Guid personId, string tenantPassword)
		{
			var ret = _currentTenantSession.CurrentSession()
				.CreateQuery("select p.tenant from PersonInfo p where p.Id=:id and p.TenantPassword=:tenantPassword")
				.SetGuid("id", personId)
				.SetString("tenantPassword", tenantPassword)
				.UniqueResult<Tenant>();
			if(ret==null)
				throw new TenantPermissionException("No permission to tenant service!");
			
			return ret;
		}
	}
}