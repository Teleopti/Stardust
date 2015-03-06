namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IApplicationUserTenantQuery
	{
		PersonInfo Find(string username);
	}
}