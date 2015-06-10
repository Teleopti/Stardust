using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class DeletePersonInfo : IDeletePersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ICurrentTenant _currentTenant;

		public DeletePersonInfo(ICurrentTenantSession currentTenantSession, ICurrentTenant currentTenant)
		{
			_currentTenantSession = currentTenantSession;
			_currentTenant = currentTenant;
		}

		public void Delete(Guid personId)
		{
			//delete with a single query if there will be perf issues here.
			//TODO: tenant

			var session = _currentTenantSession.CurrentSession();
			var personInfo = session.Get<PersonInfo>(personId);
			if (personInfo != null && personInfo.Tenant.Name== _currentTenant.Current().Name)
			{
				session.Delete(personInfo);
			}
		}
	}
}