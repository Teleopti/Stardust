using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class LoadAllPersonInfos 
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public LoadAllPersonInfos(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IEnumerable<PersonInfo> PersonInfos()
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();
		}

	}
}