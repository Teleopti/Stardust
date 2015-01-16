namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IApplicationUserQuery
	{
		ApplicationUserQueryResult FindUserData(string userName);
	}
}