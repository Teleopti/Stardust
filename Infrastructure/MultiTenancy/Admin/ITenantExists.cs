namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface ITenantExists
	{
		ImportTenantResultModel Check(string tenantName);
		bool CheckNewName(string newTenantName, string oldTenantName);
	}
}