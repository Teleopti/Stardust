using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepository : AnalyticsDateRepositoryBase, IAnalyticsDateRepository
	{
		public AnalyticsDateRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork) : base(analyticsUnitOfWork)
		{
		}

		public new IAnalyticsDate MaxDate()
		{
			return base.MaxDate();
		}

		public new IAnalyticsDate MinDate()
		{
			return base.MinDate();
		}

		public new IAnalyticsDate Date(DateTime dateDate)
		{
			return base.Date(dateDate);
		}
	}

	public abstract class AnalyticsDateRepositoryBase
	{
		protected readonly ICurrentAnalyticsUnitOfWork AnalyticsUnitOfWork;

		protected AnalyticsDateRepositoryBase(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			AnalyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IAnalyticsDate MaxDate()
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT TOP 1 date_id {nameof(IAnalyticsDate.DateId)}, date_date {nameof(IAnalyticsDate.DateDate)}
					FROM [mart].[dim_date] WITH (NOLOCK)
					WHERE date_id >= 0
					ORDER BY date_id DESC")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate MinDate()
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT TOP 1 date_id {nameof(IAnalyticsDate.DateId)}, date_date {nameof(IAnalyticsDate.DateDate)}
					FROM [mart].[dim_date] WITH (NOLOCK)
					WHERE date_id >= 0
					ORDER BY date_id ASC")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			return AnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
						date_id {nameof(IAnalyticsDate.DateId)}, 
						date_date {nameof(IAnalyticsDate.DateDate)} 
					FROM mart.dim_date WITH (NOLOCK) 
					WHERE date_date=:{nameof(dateDate)}")
				.SetDateTime(nameof(dateDate), dateDate.Date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}
	}
}
