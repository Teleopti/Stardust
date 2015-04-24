namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	//TODO: tenant rename -> no need to have Tenant in its name
	public interface IApplicationUserTenantQuery
	{
		PersonInfo Find(string username);
	}
}