using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindTenantByName : IFindTenantByName
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindTenantByName(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public Tenant Find(string name)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findByName")
				.SetString("name", name)
				.UniqueResult<Tenant>();
		}
	}
}