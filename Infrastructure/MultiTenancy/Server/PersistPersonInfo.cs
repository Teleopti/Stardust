using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersistPersonInfo : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();

			var oldPersonInfo = session.Get<PersonInfo>(personInfo.Id);
			if (oldPersonInfo == null)
			{
				session.Save(personInfo);
			}
			else
			{
				session.Merge(personInfo);
			}
		}
	}
}