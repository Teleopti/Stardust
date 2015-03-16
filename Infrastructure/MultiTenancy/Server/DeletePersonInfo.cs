using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DeletePersonInfo : IDeletePersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public DeletePersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		//TODO: tenant make this smarter. Accept enumerable instead and make query instead?
		public void Delete(Guid personId)
		{
			var session = _currentTenantSession.CurrentSession();
			var personInfo = session.Get<PersonInfo>(personId);
			if (personInfo != null)
			{
				session.Delete(personInfo);
			}
		}
	}
}