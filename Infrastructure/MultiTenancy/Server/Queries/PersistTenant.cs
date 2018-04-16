﻿using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistTenant
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistTenant(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(Tenant tenant)
		{
			_currentTenantSession.CurrentSession().Save(tenant);
		}

		public void Persist(PersonInfo personInfo)
		{
			_currentTenantSession.CurrentSession().Save(personInfo);
		}
	}
}