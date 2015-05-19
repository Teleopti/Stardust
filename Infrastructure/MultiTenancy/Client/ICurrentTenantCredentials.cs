namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ICurrentTenantCredentials
	{
		TenantCredentials TenantCredentials();
	}
}