using System;
using NHibernate.Criterion;
using NHibernate.Proxy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class DatabaseVersion
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public DatabaseVersion(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public int? FetchFor(IAggregateRoot aggregateRoot, bool usePessimisticLock)
		{
			if (!aggregateRoot.Id.HasValue)
				throw new ArgumentException("Cannot find " + aggregateRoot + " in db");
			int result;

			var proxy = aggregateRoot as INHibernateProxy;
			var type = proxy == null ? aggregateRoot.GetType() : proxy.HibernateLazyInitializer.PersistentClass;

			var session = _currentUnitOfWork.Current().Session();
			if (usePessimisticLock)
			{
				// use hql query here when SetLockMode works on non entities. Seems to be a problem currently
				//possible sql injection here but that's pretty lĺngsökt so never mind...
				var sql = "select Version from " + type.Name + " with(updlock, holdlock) where Id =:id";
				result = session.CreateSQLQuery(sql)
					.SetGuid("id", aggregateRoot.Id.Value)
					.UniqueResult<int>();
			}
			else
			{
				result = session.CreateCriteria(type)
					.Add(Restrictions.Eq("Id", aggregateRoot.Id))
					.SetProjection(Projections.Property("Version"))
					.UniqueResult<int>();
			}
			if (result == 0)
				return null;
			return result;
		}
	}
}