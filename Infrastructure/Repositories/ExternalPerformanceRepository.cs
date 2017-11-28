using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceRepository : Repository<ExternalPerformance>, IExternalPerformanceRepository
	{
#pragma warning disable 618
		public ExternalPerformanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public ExternalPerformanceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<ExternalPerformance> FindAllExternalPerformances()
		{
			return Session.CreateCriteria(typeof(ExternalPerformance))
				.AddOrder(Order.Asc("Name"))
				.List<ExternalPerformance>();
		}

		public ExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			var result = Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Eq("ExternalId", externalId))
				.List<ExternalPerformance>()
				.SingleOrDefault();
			return result;
		}
	}
}
