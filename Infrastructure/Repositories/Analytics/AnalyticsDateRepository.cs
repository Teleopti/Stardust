using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsDateRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<KeyValuePair<DateOnly, int>> Dates()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date BETWEEN DATEADD(DAY,-365, GETDATE()) AND  DATEADD(DAY, 365, GETDATE())")
				.SetResultTransformer(new CustomDictionaryTransformer()).List<KeyValuePair<DateOnly, int>>();
		}

		public KeyValuePair<DateOnly, int> Date(DateTime date)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date=:Date")
				.SetDateTime("Date", date.Date)
				.SetResultTransformer(new CustomDictionaryTransformer())
				.UniqueResult<KeyValuePair<DateOnly, int>>();
		}
	}
}
