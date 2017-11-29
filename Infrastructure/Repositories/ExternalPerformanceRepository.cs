using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceRepository : Repository<IExternalPerformance>, IExternalPerformanceRepository
	{
#pragma warning disable 618
		public ExternalPerformanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public ExternalPerformanceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			ICollection<IExternalPerformance> result = Session.CreateCriteria(typeof(ExternalPerformance))
				.Add(Restrictions.Eq("IsDeleted", false))
				.AddOrder(Order.Asc("Name"))
				.List<IExternalPerformance>();

			return result;
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			var result = Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Eq("ExternalId", externalId))
				.Add(Restrictions.Eq("IsDeleted", false))
				.List<IExternalPerformance>()
				.SingleOrDefault();
			return result;
		}
	}
}
