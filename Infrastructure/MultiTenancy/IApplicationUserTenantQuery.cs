namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IApplicationUserTenantQuery
	{
		PersonInfo Find(string username);
	}
}