using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindTenantNameByRtaKey : IFindTenantNameByRtaKey
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindTenantNameByRtaKey(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public string Find(string rtaKey)
		{
			return _currentTenantSession.CurrentSession()
				.Query<Tenant>()
				.Where(x => x.RtaKey == rtaKey)
				.Select(x => x.Name)
				.SingleOrDefault();
		}
	}
}