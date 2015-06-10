namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IApplicationUserQuery
	{
		PersonInfo Find(string username);
	}
}