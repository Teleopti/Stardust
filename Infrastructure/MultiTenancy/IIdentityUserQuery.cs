namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IIdentityUserQuery
	{
		ApplicationUserQueryResult FindUserData(string userName);
	}
}