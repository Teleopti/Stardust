using System.Collections.Generic;
using NHibernate;
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
			ICriteria crit = session.CreateCriteria(typeof(CryptoKeyInfo))
				.Add(Restrictions.Eq("Bucket", bucket))
				.Add(Restrictions.Eq("Handle", handle));
			return crit.UniqueResult<CryptoKeyInfo>();
		}

		public IEnumerable<CryptoKeyInfo> Find(string bucket)
		{
			var session = _currentTenantSession.CurrentSession();
			ICriteria crit = session.CreateCriteria(typeof (CryptoKeyInfo))
				.Add(Restrictions.Eq("Bucket", bucket));
			return crit.List<CryptoKeyInfo>();
		}

		public void Add(CryptoKeyInfo cryptoKeyInfo)
		{
			_currentTenantSession.CurrentSession().SaveOrUpdate(cryptoKeyInfo);
		}

		public void Remove(string bucket, string handle)
		{
			var cryptoKeyInfo = Find(bucket, handle);
			if (cryptoKeyInfo != null)
				_currentTenantSession.CurrentSession().Delete(cryptoKeyInfo);
		}
	}
}