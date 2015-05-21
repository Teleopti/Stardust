namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ICurrentTenantUser
	{
		PersonInfo CurrentUser();
	}
}