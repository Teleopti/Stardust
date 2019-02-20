using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceRepository : Repository<IExternalPerformance>, IExternalPerformanceRepository
	{
		public ExternalPerformanceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{
		}

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			var businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current();
			return Session.CreateCriteria(typeof(ExternalPerformance))
				.Add(Restrictions.Eq("BusinessUnit", businessUnit))
				.AddOrder(Order.Asc("Name"))
				.List<IExternalPerformance>();
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			var businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current();
			return Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Eq("BusinessUnit", businessUnit))
				.Add(Restrictions.Eq("ExternalId", externalId))
				.UniqueResult<IExternalPerformance>();
		}
	}
}
