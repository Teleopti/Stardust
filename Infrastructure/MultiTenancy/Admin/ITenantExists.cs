namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface ITenantExists
	{
		bool Check(string tenantName);
	}
}