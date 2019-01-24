namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IChangePasswordTenantClient
	{
		ChangePasswordResult SetNewPassword(ChangePasswordInput newPasswordInput);
	}
}