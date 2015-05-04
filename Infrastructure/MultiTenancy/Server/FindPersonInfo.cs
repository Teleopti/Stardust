using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindPersonInfo : IFindPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindPersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo GetById(Guid id)
		{
			return _currentTenantSession.CurrentSession()
				.Get<PersonInfo>(id);
		}
	}

	//TODO: tenant remove me when old schema is gone!
	public class FindPersonInfo_OldSchema : IFindPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindPersonInfo_OldSchema(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo GetById(Guid id)
		{
			return (PersonInfo) _currentTenantSession.CurrentSession()
				.Get("PersonInfo_OldSchema", id);
		}
	}
}