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

	public class FindPersonInfoByCredentials_UseUntilToggleIsGone : IFindPersonInfoByCredentials
	{
		private readonly IFindPersonInfo _findPersonInfo;

		public FindPersonInfoByCredentials_UseUntilToggleIsGone(IFindPersonInfo findPersonInfo)
		{
			_findPersonInfo = findPersonInfo;
		}

		public PersonInfo Find(Guid personId, string tenantPassword)
		{
			return _findPersonInfo.GetById(personId);
		}
	}
}