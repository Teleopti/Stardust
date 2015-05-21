using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DeletePersonInfo : IDeletePersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ICurrentTenantUser _currentTenantUser;

		public DeletePersonInfo(ICurrentTenantSession currentTenantSession, ICurrentTenantUser currentTenantUser)
		{
			_currentTenantSession = currentTenantSession;
			_currentTenantUser = currentTenantUser;
		}

		public void Delete(Guid personId)
		{
			var session = _currentTenantSession.CurrentSession();
			var personInfo = session.Get<PersonInfo>(personId);
			if (personInfo != null && personInfo.Tenant.Name==_currentTenantUser.CurrentUser().Tenant.Name)
			{
				session.Delete(personInfo);
			}
		}
	}
}