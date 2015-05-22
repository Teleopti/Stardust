namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ICurrentTenant
	{
		Tenant Current();
	}
}