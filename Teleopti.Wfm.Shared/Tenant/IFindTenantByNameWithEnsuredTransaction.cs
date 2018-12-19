namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindTenantByNameWithEnsuredTransaction
	{
		Tenant Find(string name);
	}
}