using System;
using System.Collections.Generic;
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
				.Add(Restrictions.Eq("Bucket", bucket))
				.Add(Restrictions.Eq("Handle", handle));
			return crit.UniqueResult<CryptoKeyInfo>();
		}

		public IEnumerable<CryptoKeyInfo> Find(string bucket)
		{
			var session = _currentTenantSession.CurrentSession();
			var crit = session.CreateCriteria<CryptoKeyInfo>()
				.Add(Restrictions.Eq("Bucket", bucket));
			return crit.List<CryptoKeyInfo>();
		}

		public void Add(CryptoKeyInfo cryptoKeyInfo)
		{
			if (Find(cryptoKeyInfo.Bucket, cryptoKeyInfo.Handle) != null)
				throw new DuplicateCryptoKeyException();
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
			session.CreateQuery("DELETE FROM CryptoKeyInfo c WHERE c.CryptoKeyExpiration < :expiredTimestamp")
				.SetParameter("expiredTimestamp", expiredTimestamp)
				.ExecuteUpdate();
		}
	}

	public class DuplicateCryptoKeyException : Exception
	{
	}
}