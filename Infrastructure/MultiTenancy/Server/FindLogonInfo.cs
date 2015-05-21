using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindLogonInfo : IFindLogonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ICurrentTenantUser _currentTenantUser;

		public FindLogonInfo(ICurrentTenantSession currentTenantSession, ICurrentTenantUser currentTenantUser)
		{
			_currentTenantSession = currentTenantSession;
			_currentTenantUser = currentTenantUser;
		}

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findLogonInfo")
				.SetResultTransformer(Transformers.AliasToBean<LogonInfo>())
				.SetParameterList("ids", ids)
				.SetEntity("tenant", _currentTenantUser.CurrentUser().Tenant)
				.List<LogonInfo>();
		}
	}
}