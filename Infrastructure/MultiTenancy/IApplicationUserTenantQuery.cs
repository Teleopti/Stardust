namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	//TODO: tenant - remove this when there is one "path" for identity and app user when doing the "start part" of logon process
	public interface IApplicationUserTenantQuery
	{
		PersonInfo Find(string username);
	}
}