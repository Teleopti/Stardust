using NHibernate.Util;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class RegenerateAllTenantPasswords : IRegenerateAllTenantPasswords
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public RegenerateAllTenantPasswords(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Modify()
		{
			_currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>()
				.ForEach(pi => pi.RegenerateTenantPassword());
		} 
	}
}