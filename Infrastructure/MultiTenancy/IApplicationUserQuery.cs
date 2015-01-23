namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IApplicationUserQuery
	{
		PasswordPolicyForUser FindUserData(string userName);
	}
}