namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindTenantByName
	{
		Tenant Find(string name);
	}
}