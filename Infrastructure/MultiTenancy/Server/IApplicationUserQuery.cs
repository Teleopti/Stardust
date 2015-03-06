namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IApplicationUserQuery
	{
		PasswordPolicyForUser FindUserData(string userName);
	}
}