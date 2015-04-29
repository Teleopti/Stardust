using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindLogonInfo : IFindLogonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindLogonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findLogonInfo")
				.SetResultTransformer(Transformers.AliasToBean<LogonInfo>())
				.SetParameterList("ids", ids)
				.List<LogonInfo>();
		}
	}
}