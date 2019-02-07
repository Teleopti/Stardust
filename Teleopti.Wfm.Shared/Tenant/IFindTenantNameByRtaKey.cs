namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IFindTenantNameByRtaKey
	{
		string Find(string rtaKey);
	}
}