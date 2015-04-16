namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IChangeUserPassword
	{
		ChangeUserPasswordResult SetNewPassword(ChangePasswordInput newPasswordInput);
	}
}