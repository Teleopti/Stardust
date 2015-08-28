using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindTenantByRtaKey : IFindTenantByRtaKey
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindTenantByRtaKey(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public Tenant Find(string rtaKey)
		{
			return _currentTenantSession.CurrentSession()
				.QueryOver<Tenant>()
				.Where(x => x.RtaKey == rtaKey)
				.SingleOrDefault();
		}
	}
}