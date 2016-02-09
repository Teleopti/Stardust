using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class CryptoKeyInfoRepository : ICryptoKeyInfoRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public CryptoKeyInfoRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public CryptoKeyInfo Find(string bucket, string handle)
		{
			var session = _currentUnitOfWork.Current().Session();
			ICriteria crit = session.CreateCriteria(typeof(CryptoKeyInfo))
				.Add(Restrictions.Eq("Bucket", bucket))
				.Add(Restrictions.Eq("Handle", handle));
			return crit.UniqueResult<CryptoKeyInfo>();
		}

		public IEnumerable<CryptoKeyInfo> Find(string bucket)
		{
			var session = _currentUnitOfWork.Current().Session();
			ICriteria crit = session.CreateCriteria(typeof (CryptoKeyInfo))
				.Add(Restrictions.Eq("Bucket", bucket));
			return crit.List<CryptoKeyInfo>();
		}

		public void Add(CryptoKeyInfo cryptoKeyInfo)
		{
			_currentUnitOfWork.Current().Session().SaveOrUpdate(cryptoKeyInfo);
		}

		public void Remove(string bucket, string handle)
		{
			var cryptoKeyInfo = Find(bucket, handle);
			if (cryptoKeyInfo != null)
				_currentUnitOfWork.Current().Session().Delete(cryptoKeyInfo);
		}
	}
}