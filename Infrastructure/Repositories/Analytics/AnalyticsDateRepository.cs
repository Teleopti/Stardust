using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsDateRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IList<KeyValuePair<DateOnly, int>> Dates()
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date BETWEEN DATEADD(DAY,-365, GETDATE()) AND  DATEADD(DAY, 365, GETDATE())")
					.SetResultTransformer(new CustomDictionaryTransformer()).List<KeyValuePair<DateOnly, int>>();
			}
		}

		public KeyValuePair<DateOnly, int> Date(DateTime date)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date=:Date")
					.SetDateTime("Date", date.Date)
					.SetResultTransformer(new CustomDictionaryTransformer())
					.UniqueResult<KeyValuePair<DateOnly, int>>();
			}
		}
	}
}
