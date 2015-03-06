namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindTenantAndPersonIdForIdentity
	{
		TenantAndPersonId Find(string identity);
	}
}