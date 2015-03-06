namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IIdentityUserQuery
	{
		PersonInfo FindUserData(string identity);
	}
}