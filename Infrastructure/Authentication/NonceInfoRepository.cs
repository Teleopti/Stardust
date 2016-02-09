using System;
using NHibernate.Criterion;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class NonceInfoRepository : INonceInfoRepository
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public NonceInfoRepository(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public NonceInfo Find(string context, string nonce, DateTime timestamp)
		{
			var session = _currentTenantSession.CurrentSession();
			var crit = session.CreateCriteria(typeof(NonceInfo))
				.Add(Restrictions.Eq("Context", context))
				.Add(Restrictions.Eq("Nonce", nonce))
				.Add(Restrictions.Eq("Timestamp", timestamp));
			return crit.UniqueResult<NonceInfo>();
		}

		public void Add(NonceInfo nonceInfo)
		{
			_currentTenantSession.CurrentSession().SaveOrUpdate(nonceInfo);
		}
	}
}