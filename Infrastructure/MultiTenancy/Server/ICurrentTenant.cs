namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ICurrentTenant
	{
		Tenant Current();
	}

	public class CurrentTenant : ICurrentTenant
	{
		private readonly ICurrentTenantUser _currentTenantUser;

		public CurrentTenant(ICurrentTenantUser currentTenantUser)
		{
			_currentTenantUser = currentTenantUser;
		}

		public Tenant Current()
		{
			return _currentTenantUser.CurrentUser().Tenant;
		}
	}
}