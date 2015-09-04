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

		public ImportTenantResultModel Check(string tenantName)
		{
			if (string.IsNullOrEmpty(tenantName))
				return new ImportTenantResultModel { Message = "You must enter a new name for the Tenant!", Success = false };

			var exists = _currentTenantSession.CurrentSession()
				.CreateQuery("select count(id) from Tenant t where t.Name=:name")
				.SetString("name", tenantName)
				.UniqueResult<long>() == 1;

			if (exists)
				return new ImportTenantResultModel { Message = "There is already a Tenant with this name!", Success = false };

			return new ImportTenantResultModel { Message = "There is no other Tenant with this name!", Success = true };
		}

		public bool CheckNewName(string newTenantName, string oldTenantName)
		{
			return _currentTenantSession.CurrentSession()
				.CreateQuery("select count(id) from Tenant t where t.Name=:newTenantName and t.Name != :oldTenantName ")
				.SetString("newTenantName", newTenantName)
				.SetString("oldTenantName", oldTenantName)
				.UniqueResult<long>() == 1;
		}
	}
}