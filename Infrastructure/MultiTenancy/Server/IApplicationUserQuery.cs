namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IApplicationUserQuery
	{
		PersonInfo Find(string username);
	}
}