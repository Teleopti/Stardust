namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IFindTenantAndPersonIdForIdentity
	{
		TenantAndPersonId Find(string identity);
	}
}