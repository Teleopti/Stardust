namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindTenantByRtaKey
	{
		Tenant Find(string rtaKey);
	}
}