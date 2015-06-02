using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindPersonInfoByCredentials : IFindPersonInfoByCredentials
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindPersonInfoByCredentials(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo Find(Guid personId, string tenantPassword)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findUserByCredentials")
				.SetGuid("id", personId)
				.SetString("tenantPassword", tenantPassword)
				.UniqueResult<PersonInfo>();
		}
	}
}