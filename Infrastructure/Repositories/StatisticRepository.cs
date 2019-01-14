using System.ComponentModel;
using log4net;
using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for Matrix stuff
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-22
	/// </remarks>
	public class StatisticRepository : IStatisticRepository
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(StatisticRepository));

		internal StatisticRepository()
		{
		}

		/// <summary>
		/// Load matrix reports
		/// </summary>
		/// <returns></returns>
		public ICollection<MatrixReportInfo> LoadReports()
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query = createReportQuery(uow);
				return query.List<MatrixReportInfo>();
			}
		}

		/// <summary>
		/// Loads the queues from Matrix.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-07
		/// </remarks>
		public ICollection<IQueueSource> LoadQueues()
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query = createQueueQuery(uow);
				return query.List<IQueueSource>();
			}
		}

		/// <summary>
		/// Loads the specific dates.
		/// </summary>
		/// <param name="sources"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources,
														DateTimePeriod period)
		{
			if (sources.Count == 0) return new List<IStatisticTask>();

			List<IStatisticTask> statisticTasks = new List<IStatisticTask>();

			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var source in sources.Batch(500))
				{
					string queueList = buildStringQueueList(source);
					IQuery query = createQuery(uow, period, queueList);
					query.SetTimeout(1200);

					statisticTasks.AddRange(query.List<IStatisticTask>());
				}

				return statisticTasks;
			}
		}

		public ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources,
			DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			if (sources.Count == 0) return new List<IStatisticTask>();

			var statisticTasks = new List<IStatisticTask>();

			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var source in sources.Batch(500))
				{
					string queueList = buildStringQueueList(source);
					IQuery query = createDailyStatisticQuery(uow, period, queueList, timeZoneId, midnightBreakOffset);
					query.SetTimeout(1200);

					statisticTasks.AddRange(query.List<IStatisticTask>());
				}

				return statisticTasks;
			}
		}

		public DateOnlyPeriod? QueueStatisticsUpUntilDate(ICollection<IQueueSource> sources)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var startDate = DateTime.MinValue;
				var endDate = DateTime.MaxValue;
				foreach (var source in sources.Batch(500))
				{
					var queueList = buildStringQueueList(source);
					var date = session(uow).CreateSQLQuery("exec mart.raptor_queue_statistics_up_until_date @QueueList=:QueueList")
						.SetString("QueueList", queueList)
						.SetResultTransformer(Transformers.AliasToBean(typeof(StatisticPeriod)))
						.UniqueResult<StatisticPeriod>();

					if (date.StartDate.HasValue && date.StartDate.Value > startDate)
						startDate = date.StartDate.Value;
					if (date.EndDate.HasValue && date.EndDate.Value < endDate)
						endDate = date.EndDate.Value;
				}

				if (startDate == DateTime.MinValue)
					return null;
				return new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			}
		}

		public ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query = createAgentCountQuery(uow, skill, period);
				return query.List<IActiveAgentCount>();
			}
		}

		public void PersistFactQueues(DataTable queueDataTable)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				var dbConnection = uow.Session().Connection.Unwrap();
				using (SqlBulkCopy bulkCopy = new SqlBulkCopy(dbConnection))
				{
					bulkCopy.DestinationTableName = "stage.stg_queue";
					bulkCopy.BulkCopyTimeout = 600;
					bulkCopy.WriteToServer(queueDataTable);
				}
			}
		}

		private static string buildStringQueueList(IEnumerable<IQueueSource> sources)
		{
			return String.Join(",", sources.Select(s => s
				 .QueueMartId.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		private IQuery createDailyStatisticQuery(IStatelessUnitOfWork uow, DateTimePeriod date, string queueList, string timeZoneId,
			TimeSpan midnightBreakOffset)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_queue_statistics_load @DateFrom=:DateFrom, @DateTo=:DateTo, "
				+ "@QueueList=:QueueList, @TimeZoneCode=:TimeZoneId, @MidnightBreakDifference=:MidnightBreakDifference")
				.AddScalar("StatAverageTaskTimeSeconds", NHibernateUtil.Double)
				.AddScalar("StatAverageAfterTaskTimeSeconds", NHibernateUtil.Double)
				.AddScalar("StatOfferedTasks", NHibernateUtil.Double)
				.AddScalar("StatAnsweredTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedShortTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedTasksWithinSL", NHibernateUtil.Double)
				.AddScalar("StatOverflowOutTasks", NHibernateUtil.Double)
				.AddScalar("StatOverflowInTasks", NHibernateUtil.Double)
				.AddScalar("Interval", NHibernateUtil.DateTime)
				.SetReadOnly(true)
				.SetString("DateFrom", date.StartDateTime.ToString(CultureInfo.InvariantCulture))
				.SetString("DateTo", date.EndDateTime.ToString(CultureInfo.InvariantCulture))
				.SetString("QueueList", queueList)
				.SetString("TimeZoneId", timeZoneId)
				.SetString("MidnightBreakDifference", midnightBreakOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture))
				.SetResultTransformer(Transformers.AliasToBean(typeof(StatisticTask)));
		}

		private static IQuery createQuery(IStatelessUnitOfWork uow, DateTimePeriod date, string queueList)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_statistics_load @DateFrom=:DateFrom,@DateTo=:DateTo, @QueueList=:QueueList")
				 .AddScalar("StatAverageTaskTimeSeconds", NHibernateUtil.Double)
				 .AddScalar("StatAverageAfterTaskTimeSeconds", NHibernateUtil.Double)
				 .AddScalar("StatOfferedTasks", NHibernateUtil.Double)
				 .AddScalar("StatAnsweredTasks", NHibernateUtil.Double)
			.AddScalar("StatAbandonedTasks", NHibernateUtil.Double)

			.AddScalar("StatAbandonedShortTasks", NHibernateUtil.Double)
			.AddScalar("StatAbandonedTasksWithinSL", NHibernateUtil.Double)
			.AddScalar("StatAnsweredTasksWithinSL", NHibernateUtil.Double)
			.AddScalar("StatOverflowOutTasks", NHibernateUtil.Double)
			.AddScalar("StatOverflowInTasks", NHibernateUtil.Double)
			.AddScalar("StatAverageQueueTimeSeconds", NHibernateUtil.Int32)
			.AddScalar("StatAverageHandleTimeSeconds", NHibernateUtil.Double)
			.AddScalar("StatAverageTimeToAbandonSeconds", NHibernateUtil.Int32)
			.AddScalar("StatAverageTimeLongestInQueueAnsweredSeconds", NHibernateUtil.Int32)
			.AddScalar("StatAverageTimeLongestInQueueAbandonedSeconds", NHibernateUtil.Int32)

				 .AddScalar("Interval", NHibernateUtil.DateTime)
				 .SetReadOnly(true)
				 .SetString("DateFrom", date.StartDateTime.ToString(CultureInfo.InvariantCulture))
				 .SetString("DateTo", date.EndDateTime.ToString(CultureInfo.InvariantCulture))
				 .SetString("QueueList", queueList)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(StatisticTask)));
		}

		private static IQuery createAgentCountQuery(IStatelessUnitOfWork uow, ISkill skill, DateTimePeriod period)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_load_agent_count @skill=:skill,@start_date=:start_date, "
				+ "@end_date=:end_date")
				 .AddScalar("ActiveAgents", NHibernateUtil.Int32)
				 .AddScalar("Interval", NHibernateUtil.DateTime)
				 .SetReadOnly(true)
				 .SetGuid("skill", skill.Id.GetValueOrDefault())
				 .SetDateTime("start_date", period.StartDateTime)
				 .SetDateTime("end_date", period.EndDateTime)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(ActiveAgentCount)));
		}

		private static IQuery createReportQuery(IStatelessUnitOfWork uow)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_reports_load")
				 .AddScalar("ReportId", NHibernateUtil.Guid)
				 .AddScalar("ReportName", NHibernateUtil.String)
				 .AddScalar("ReportUrl", NHibernateUtil.String)
				 .AddScalar("TargetFrame", NHibernateUtil.String)
				 .AddScalar("Version", NHibernateUtil.String)
				 .SetReadOnly(true)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(MatrixReportInfo)));
		}

		private static IQuery createQueueQuery(IStatelessUnitOfWork uow)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_load_queues")
				 .AddScalar("Name", NHibernateUtil.String)
				 .AddScalar("Description", NHibernateUtil.String)
				 .AddScalar("QueueOriginalId", NHibernateUtil.Int32)
				 .AddScalar("DataSourceId", NHibernateUtil.Int32)
				 .AddScalar("QueueAggId", NHibernateUtil.Int32)
				 .AddScalar("LogObjectName", NHibernateUtil.String)
				 .AddScalar("QueueMartId", NHibernateUtil.Int32)
				 .SetReadOnly(true)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(QueueSource)));
		}

		private static IStatelessSession session(IStatelessUnitOfWork uow)
		{
			return ((NHibernateStatelessUnitOfWork)uow).Session;
		}

		public void DeleteStgQueues()
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query =
					 session(uow).CreateSQLQuery(
						  "exec mart.raptor_v_stg_queue_delete");
				query.SetTimeout(300);
				query.ExecuteUpdate();
			}
		}

		public void LoadFactQueues()
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query =
					 session(uow).CreateSQLQuery(
						  "exec mart.raptor_fact_queue_load");
				query.SetTimeout(300);
				query.ExecuteUpdate();
			}
		}

		public void LoadDimQueues()
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query =
					 session(uow).CreateSQLQuery(
						  "exec mart.raptor_dim_queue_load");
				query.SetTimeout(300);
				query.ExecuteUpdate();
			}
		}

		public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId,
			int adherenceId)
		{
			return repositoryActionWithRetry(uow => session(uow).CreateSQLQuery(
					"exec mart.raptor_adherence_report_load @date_from=:date_from, @time_zone_id=:time_zone_id, @person_code=:person_code, "
					+ "@agent_person_code=:agent_person_code, @language_id=:language_id, @adherence_id=:adherence_id")
				.SetReadOnly(true)
				.SetDateTime("date_from", dateTime)
				.SetString("time_zone_id", timeZoneId)
				.SetGuid("person_code", personCode)
				.SetGuid("agent_person_code", agentPersonCode)
				.SetInt32("language_id", languageId)
				.SetInt32("adherence_id", adherenceId)
				.List());
		}

		public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			return repositoryActionWithRetry(uow => session(uow).CreateSQLQuery(
					"exec mart.raptor_stat_agent @scenario_code=:scenario_code, @date_from=:date_from, @date_to=:date_to, "
					+ "@time_zone_code=:time_zone_code, @person_code=:person_code")
				.SetReadOnly(true)
				.SetGuid("scenario_code", scenarioCode)
				.SetDateTime("date_from", startDate)
				.SetDateTime("date_to", endDate)
				.SetString("time_zone_code", timeZoneId)
				.SetGuid("person_code", personCode)
				.List());
		}

		public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			return repositoryActionWithRetry(uow => session(uow).CreateSQLQuery(
					"exec mart.raptor_stat_agent_queue @date_from=:date_from, @date_to=:date_to, @time_zone_code=:time_zone_code, "
					+ "@person_code=:person_code")
				.SetReadOnly(true)
				.SetDateTime("date_from", startDate)
				.SetDateTime("date_to", endDate)
				.SetString("time_zone_code", timeZoneId)
				.SetGuid("person_code", personCode)
				.List());
		}

		public IEnumerable<HistoricalDataDetail> GetLogObjectDetails()
		{
			return repositoryActionWithRetry(uow =>
			{
				const string sql = "SELECT obj.log_object_id AS LogObjectId\r\n"
								   + "     , log_object_desc AS LogObjectName\r\n"
								   + "	   , obj.intervals_per_day AS IntervalsPerDay\r\n"
								   + "	   , det.detail_id AS DetailId\r\n"
								   + "	   , detail_desc AS DetailName\r\n"
								   + "	   , date_value AS DateValue\r\n"
								   + "	   , int_value AS IntervalValue\r\n"
								   + "  FROM mart.v_log_object obj\r\n"
								   + " INNER JOIN mart.v_log_object_detail det\r\n"
								   + "    ON obj.log_object_id = det.log_object_id";

				return ((NHibernateStatelessUnitOfWork) uow).Session.CreateSQLQuery(sql)
					.AddScalar("LogObjectId", NHibernateUtil.Int32)
					.AddScalar("LogObjectName", NHibernateUtil.String)
					.AddScalar("IntervalsPerDay", NHibernateUtil.Int32)
					.AddScalar("DetailId", NHibernateUtil.Int32)
					.AddScalar("DetailName", NHibernateUtil.String)
					.AddScalar("DateValue", NHibernateUtil.DateTime)
					.AddScalar("IntervalValue", NHibernateUtil.Int32)
					.SetReadOnly(true)
					.List<object[]>()
					.Select(x => new HistoricalDataDetail
					{
						LogObjectId = (int) x[0],
						LogObjectName = (string) x[1],
						IntervalsPerDay = (int) x[2],
						DetailId = (int) x[3],
						DetailName = (string) x[4],
						DateValue = (DateTime) x[5],
						IntervalValue = (int) x[6]
					});
			});
		}

		private TResult repositoryActionWithRetry<TResult>(Func<IStatelessUnitOfWork, TResult> innerAction, int attempt = 0)
		{
			try
			{
				using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
				{
					var ret = innerAction(uow);
					return ret;
				}
			}
			catch (SqlException ex)
			{
				if (ex.InnerException is Win32Exception && attempt < 6)
				{
					_logger.Warn($"Retry - Count:{attempt}, Exception:{ex}, StackTrace:{ex.StackTrace}");
					return repositoryActionWithRetry(innerAction, ++attempt);
				}

				throw;
			}
		}

		private IAnalyticsUnitOfWorkFactory StatisticUnitOfWorkFactory()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			return identity.DataSource.Analytics;
		}
	}

	public class StatisticPeriod
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class IntradayStatistics: IIntradayStatistics
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public DateTime Interval { get; set; }
		public double StatAnsweredTasks { get; set; }
		public double StatOfferedTasks { get; set; }
		public double StatAbandonedTasks { get; set; }
		public double StatAbandonedShortTasks { get; set; }
		public double StatAbandonedTasksWithinSL { get; set; }
		public double StatOverflowOutTasks { get; set; }
		public double StatOverflowInTasks { get; set; }
		public double StatAverageTaskTimeSeconds { get; set; }
		public double StatAverageAfterTaskTimeSeconds { get; set; }
	}
}