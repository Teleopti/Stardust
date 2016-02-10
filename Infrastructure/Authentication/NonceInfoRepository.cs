using System;
using NHibernate;
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
			var crit = session.CreateCriteria<NonceInfo>()
				.Add(Restrictions.Eq("Context", context))
				.Add(Restrictions.Eq("Nonce", nonce))
				.Add(Restrictions.Eq("Timestamp", timestamp))
                .SetLockMode(LockMode.Upgrade) // Used to avoid deadlocking
                ;
			return crit.UniqueResult<NonceInfo>();
		}

		public void Add(NonceInfo nonceInfo)
		{
            var session = _currentTenantSession.CurrentSession();
            session.SaveOrUpdate(nonceInfo);
		}

		public void ClearExpired(DateTime expiredTimestamp)
		{
            var session = _currentTenantSession.CurrentSession();
            session.CreateQuery("DELETE FROM NonceInfo n WHERE n.Timestamp < :expiredTimestamp")
                .SetParameter("expiredTimestamp", expiredTimestamp)
                .ExecuteUpdate();
        }
	}
} 