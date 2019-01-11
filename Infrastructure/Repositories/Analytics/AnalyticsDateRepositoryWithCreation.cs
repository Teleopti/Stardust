using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepositoryWithCreation : IAnalyticsDateRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly IEventPublisher _eventPublisher;
		private readonly IAnalyticsConfigurationRepository _analyticsConfigurationRepository;
		private static bool shouldPublish;
		private readonly ILog _logger = LogManager.GetLogger(typeof(AnalyticsDateRepositoryWithCreation));

		public AnalyticsDateRepositoryWithCreation(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork,
			IAnalyticsConfigurationRepository analyticsConfigurationRepository,
			IEventPublisher eventPublisher) 
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_eventPublisher = eventPublisher;
			_analyticsConfigurationRepository = analyticsConfigurationRepository;
		}
		
		public IAnalyticsDate MaxDate()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Desc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>() ?? new AnalyticsDate {DateDate = new DateTime(1999, 12, 30) };
		}

		public IAnalyticsDate MinDate()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Asc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>() ?? AnalyticsDate.NotDefined;
		}

		private IAnalyticsDate dateInDb(DateTime dateDate)
		{
			if (dateDate == AnalyticsDate.Eternity.DateDate)
				return AnalyticsDate.Eternity;
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Eq(nameof(AnalyticsDate.DateDate), dateDate.Date))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IList<IAnalyticsDate> GetAllPartial()
		{
			AnalyticsDatePartial analyticsDatePartial = null;
			return _currentAnalyticsUnitOfWork.Current().Session().QueryOver<AnalyticsDate>()
				.Where(ad => ad.DateId >= 0)
				.SelectList(list => list
					.Select(d => d.DateId).WithAlias(() => analyticsDatePartial.DateId)
					.Select(d => d.DateDate).WithAlias(() => analyticsDatePartial.DateDate)
					)
				.OrderBy(ad => ad.DateId)
				.Asc
				.TransformUsing(Transformers.AliasToBean<AnalyticsDatePartial>())
				.List<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			return dateInDb(dateDate) ?? createDatesTo(dateDate.Date);
		}

		private IAnalyticsDate createDatesTo(DateTime dateDate)
		{
			var toDate = dateDate.AddDays(42);
			var currentDay = MaxDate().DateDate;
			var culture = _analyticsConfigurationRepository.GetCulture();

			using (var session = _currentAnalyticsUnitOfWork.Current().Session().SessionFactory.OpenStatelessSession())
			using (var transaction = session.Connection.BeginTransaction())
			{
				var dt = new DataTable();
				dt.Columns.Add("date_id", typeof(int));
				dt.Columns.Add("date_date", typeof(DateTime));
				dt.Columns.Add("year", typeof(int));
				dt.Columns.Add("year_month", typeof(int));
				dt.Columns.Add("month", typeof(int));
				dt.Columns.Add("month_name", typeof(string));
				dt.Columns.Add("month_resource_key", typeof(string));
				dt.Columns.Add("day_in_month", typeof(int));
				dt.Columns.Add("weekday_number", typeof(int));
				dt.Columns.Add("weekday_name", typeof(string));
				dt.Columns.Add("weekday_resource_key", typeof(string));
				dt.Columns.Add("week_number", typeof(int));
				dt.Columns.Add("year_week", typeof(string));
				dt.Columns.Add("quarter", typeof(string));
				dt.Columns.Add("insert_date", typeof(DateTime));


				var oneDay = TimeSpan.FromDays(1);
				while ((currentDay += oneDay) <= toDate)
				{
					var day = new AnalyticsDate(currentDay, culture);
					var row = dt.NewRow();
					row["day_in_month"] = day.DayInMonth;
					row["date_date"] = day.DateDate;
					row["year"] = day.Year;
					row["year_month"] = day.YearMonth;
					row["year_week"] = day.YearWeek;
					row["month"] = day.Month;
					row["month_name"] = day.MonthName;
					row["month_resource_key"] = day.MonthResourceKey;
					row["week_number"] = day.WeekNumber;
					row["weekday_name"] = day.WeekdayName;
					row["weekday_number"] = day.WeekdayNumber;
					row["weekday_resource_key"] = day.WeekdayResourceKey;
					row["quarter"] = day.Quarter;
					row["insert_date"] = day.InsertDate;
					dt.Rows.Add(row);

					shouldPublish = true;
				}

				if (shouldPublish)
				{
					using (var sqlBulkCopy = new SqlBulkCopy(session.Connection.Unwrap(), SqlBulkCopyOptions.Default,
						(SqlTransaction) transaction))
					{
						sqlBulkCopy.DestinationTableName = "[mart].[dim_date]";
						sqlBulkCopy.BulkCopyTimeout = 300;
						sqlBulkCopy.WriteToServer(dt);
					}

					transaction.Commit();
				}
			}
			
			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				if (!shouldPublish) return;
				_eventPublisher.Publish(new AnalyticsDatesChangedEvent());
				shouldPublish = false;
			});

			var theFoundDate = dateInDb(dateDate);
			if (theFoundDate == null)
			{
				_logger.Warn($"Failed to load newly created analytics date '{dateDate}'!");
			}
			return theFoundDate;
		}
	}
}