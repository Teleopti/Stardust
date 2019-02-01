using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Insights;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class InsightsReportRepository: Repository<IInsightsReport>, IInsightsReportRepository
	{
		public InsightsReportRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IList<IInsightsReport> GetAllValidReports()
		{
			return Session.CreateCriteria(typeof(InsightsReport), "rep")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IInsightsReport>();
		}
	}
}
