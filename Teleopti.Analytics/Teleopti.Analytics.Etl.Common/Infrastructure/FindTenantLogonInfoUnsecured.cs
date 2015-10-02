using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	//special impl to bypass security in tenant service
	//if there'll be more places like this -> fix!
	//(could get tenant instance based on IDatasource name in ETL)
	public class FindTenantLogonInfoUnsecured : IFindLogonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private const string hql = @"
select pi.Id as PersonId, pi.ApplicationLogonInfo.LogonName as LogonName, pi.Identity as Identity
from PersonInfo pi 
where pi.Id in (:ids)";

		public FindTenantLogonInfoUnsecured(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return _currentTenantSession.CurrentSession().CreateQuery(hql)
				.SetParameterList("ids", ids)
				.SetResultTransformer(Transformers.AliasToBean<LogonInfo>())
				.List<LogonInfo>();
		}

		public LogonInfo GetForLogonName(string logonName)
		{
			throw new NotImplementedException();
		}

		public LogonInfo GetForIdentity(string identity)
		{
			throw new NotImplementedException();
		}
	}
}