using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ForecastDayOverrideRepository : Repository<IForecastDayOverride>, IForecastDayOverrideRepository
	{
		public ForecastDayOverrideRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{

		}

		public ICollection<IForecastDayOverride> FindRange(DateOnlyPeriod period, IWorkload workload, IScenario scenario)
		{
			InParameter.NotNull(nameof(period), period);
			InParameter.NotNull(nameof(workload), workload);
			InParameter.NotNull(nameof(scenario), scenario);

			return Session.CreateCriteria<ForecastDayOverride>("override")
				.Add(Restrictions.Eq("override.Scenario", scenario))
				.Add(Restrictions.Between("override.Date", period.StartDate, period.EndDate))
				.Add(Restrictions.Eq("override.Workload", workload))
				.List<ForecastDayOverride>().ToArray();
		}
	}
}