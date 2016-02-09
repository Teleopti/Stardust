using System;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class NonceInfoRepository : INonceInfoRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public NonceInfoRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public NonceInfo Find(string context, string nonce, DateTime timestamp)
		{
			var session = _currentUnitOfWork.Current().Session();
			var crit = session.CreateCriteria(typeof(NonceInfo))
				.Add(Restrictions.Eq("Context", context))
				.Add(Restrictions.Eq("Nonce", nonce))
				.Add(Restrictions.Eq("Timestamp", timestamp));
			return crit.UniqueResult<NonceInfo>();
		}

		public void Add(NonceInfo nonceInfo)
		{
			_currentUnitOfWork.Current().Session().SaveOrUpdate(nonceInfo);
		}
	}
}