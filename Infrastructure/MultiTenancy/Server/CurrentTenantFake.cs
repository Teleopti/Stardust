namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CurrentTenantFake : ICurrentTenant
	{
		private Tenant _currentTenant;

		public CurrentTenantFake()
		{
			_currentTenant = new Tenant("_");
		}

		public Tenant Current()
		{
			return _currentTenant;
		}

		public void Set(Tenant tenant)
		{
			_currentTenant = tenant;
		}
	}
}