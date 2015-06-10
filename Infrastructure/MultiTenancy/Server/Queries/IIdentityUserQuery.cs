namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IIdentityUserQuery
	{
		PersonInfo FindUserData(string identity);
	}
}