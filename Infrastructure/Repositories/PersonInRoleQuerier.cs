using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonInRoleQuerier : IPersonInRoleQuerier
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public PersonInRoleQuerier(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<Guid> GetPersonInRole(Guid roleId)
		{
			const string query = @"SELECT person from PersonInApplicationRole where applicationrole = :roleId";
			return session(_currentUnitOfWork.Current()).CreateSQLQuery(query)
					  .SetGuid("roleId", roleId)
					  .SetReadOnly(true)
					  .List<Guid>();
		}

		private static ISession session(IUnitOfWork uow)
		{
			return ((NHibernateUnitOfWork) uow).Session;
		}
	}

	public interface IPersonInRoleQuerier
	{
		IEnumerable<Guid> GetPersonInRole(Guid roleId);
	}
}
