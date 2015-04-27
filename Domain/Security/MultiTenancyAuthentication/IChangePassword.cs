namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IChangePassword
	{
		ChangePasswordResult SetNewPassword(ChangePasswordInput newPasswordInput);
	}
}