﻿using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindLogonInfo : IFindLogonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ICurrentTenant _currentTenant;

		public FindLogonInfo(ICurrentTenantSession currentTenantSession, ICurrentTenant currentTenant)
		{
			_currentTenantSession = currentTenantSession;
			_currentTenant = currentTenant;
		}

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findLogonInfo")
				.SetResultTransformer(Transformers.AliasToBean<LogonInfo>())
				.SetParameterList("ids", ids)
				.SetEntity("tenant", _currentTenant.Current())
				.List<LogonInfo>();
		}
	}
}