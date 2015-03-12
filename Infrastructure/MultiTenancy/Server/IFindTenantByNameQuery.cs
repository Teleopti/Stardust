namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindTenantByNameQuery
	{
		Tenant Find(string name);
	}
}