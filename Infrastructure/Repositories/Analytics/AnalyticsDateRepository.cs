using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepository : IAnalyticsDateRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsDateRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<IAnalyticsDate> Dates()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select date_id DateId, date_date DateDate from mart.dim_date WITH (NOLOCK) where date_date BETWEEN DATEADD(DAY,-365, GETDATE()) AND  DATEADD(DAY, 365, GETDATE())")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.List<IAnalyticsDate>();
		}

		//public KeyValuePair<DateOnly, int> Date(DateTime date)
		//{
		//	return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
		//		"select date_id, date_date from mart.dim_date WITH (NOLOCK) where date_date=:Date")
		//		.SetDateTime("Date", date.Date)
		//		.SetResultTransformer(new CustomDictionaryTransformer())
		//		.UniqueResult<KeyValuePair<DateOnly, int>>();
		//}

		public IAnalyticsDate MaxDate()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select max(date_id) DateId, max(date_date) DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_id>=0")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate MinDate()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select min(date_id) DateId, min(date_date) DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_id>=0")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime date)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select date_id DateId, date_date DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_date=:dateDate")
				.SetDateTime("dateDate", date.Date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}
	}
}
