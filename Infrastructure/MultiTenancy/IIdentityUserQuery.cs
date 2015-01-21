namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IIdentityUserQuery
	{
		PersonInfo FindUserData(string identity);
	}
}