using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceDataRepository : Repository<IExternalPerformanceData>, IExternalPerformanceDataRepository
	{
		public ExternalPerformanceDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ICollection<IExternalPerformanceData> FindByPeriod(DateTimePeriod period)
		{
			return Session.CreateCriteria<ExternalPerformanceData>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Ge("DateFrom", period.StartDateTime))
					.Add(Restrictions.Le("DateFrom", period.EndDateTime)))
				.List<IExternalPerformanceData>();
		}
	}
}
