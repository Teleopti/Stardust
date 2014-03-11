using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class PersonOrganizationReader : IPersonOrganizationReader
	{
		private readonly INow _now;
		private const string sqlQuery = "exec [dbo].[LoadAllPersonsCurrentBuSiteTeam] @now=:now";

		public PersonOrganizationReader(INow now)
		{
			_now = now;
		}

		public IEnumerable<PersonOrganizationData> LoadAll()
		{
			//inject ICurrentUnitOfWork and handle transaction from outside later
			//rta client needs to be aware of IUnitOfWork first!
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(sqlQuery)
				   .SetDateTime("now", _now.UtcDateTime())
				   .SetResultTransformer(Transformers.AliasToBean<PersonOrganizationData>())
				   .List<PersonOrganizationData>();
			}
		}
	}
}