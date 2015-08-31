namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindTenantNameByRtaKey
	{
		string Find(string rtaKey);
	}
}