using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class CryptoKeyInfoRepository : ICryptoKeyInfoRepository
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public CryptoKeyInfoRepository(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public CryptoKeyInfo Find(string bucket, string handle)
		{
			var session = _currentTenantSession.CurrentSession();
			var crit = session.CreateCriteria<CryptoKeyInfo>()
				.Add(Restrictions.Eq(nameof(CryptoKeyInfo.Bucket), bucket))
				.Add(Restrictions.Eq(nameof(CryptoKeyInfo.Handle), handle))
				.Add(Restrictions.Gt(nameof(CryptoKeyInfo.CryptoKeyExpiration), DateTime.UtcNow))
				;
			return crit.UniqueResult<CryptoKeyInfo>();
		}

		public IEnumerable<CryptoKeyInfo> Find(string bucket)
		{
			var session = _currentTenantSession.CurrentSession();
			var crit = session.CreateCriteria<CryptoKeyInfo>()
				.Add(Restrictions.Eq(nameof(CryptoKeyInfo.Bucket), bucket))
				.Add(Restrictions.Gt(nameof(CryptoKeyInfo.CryptoKeyExpiration), DateTime.UtcNow));
			return crit.List<CryptoKeyInfo>().OrderByDescending(x => x.CryptoKeyExpiration);
		}

		public void Add(CryptoKeyInfo cryptoKeyInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			session.SaveOrUpdate(cryptoKeyInfo);
		}

		public void Remove(string bucket, string handle)
		{
			var cryptoKeyInfo = Find(bucket, handle);
			if (cryptoKeyInfo != null)
				_currentTenantSession.CurrentSession().Delete(cryptoKeyInfo);
		}

		public void ClearExpired(DateTime expiredTimestamp)
		{
			var session = _currentTenantSession.CurrentSession();
			session.CreateQuery($"DELETE FROM {nameof(CryptoKeyInfo)} c WHERE c.{nameof(CryptoKeyInfo.CryptoKeyExpiration)} < :{nameof(expiredTimestamp)}")
				.SetParameter(nameof(expiredTimestamp), expiredTimestamp)
				.ExecuteUpdate();
		}
	}
}