using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		internal StatisticRepository() { }

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

			ICollection<IStatisticTask> statisticTasks = new Collection<IStatisticTask>();

			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var source in sources.Batch(500))
				{
					string queueList = buildStringQueueList(source);
					IQuery query = createQuery(uow, period, queueList);
					query.SetTimeout(1200);

					foreach (var item in query.List<IStatisticTask>())
					{
						statisticTasks.Add(item);
					}
				}

				return statisticTasks;
			}
		}

		public ICollection<IStatisticTask> LoadDailyStatisticForSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			if (sources.Count == 0) return new List<IStatisticTask>();

			ICollection<IStatisticTask> statisticTasks = new Collection<IStatisticTask>();

			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var source in sources.Batch(500))
				{
					string queueList = buildStringQueueList(source);
					IQuery query = createDailyStatisticQuery(uow, period, queueList, timeZoneId, midnightBreakOffset);
					query.SetTimeout(1200);

					foreach (var item in query.List<IStatisticTask>())
					{
						statisticTasks.Add(item);
					}
				}

				return statisticTasks;
			}
		}

		public ICollection<IIntradayStatistics> LoadSkillStatisticForSpecificDates(DateTimePeriod period, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				IQuery query = createSkillIntervalStatisticQuery(uow, period, timeZoneId, midnightBreakOffset);
				query.SetTimeout(1200);
				return query.SetResultTransformer(Transformers.AliasToBean(typeof(IntradayStatistics))).List<IIntradayStatistics>();
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
				IDbConnection dbConnection = ((NHibernateStatelessUnitOfWork)uow).Session.Connection;

				using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)dbConnection))
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

		private IQuery createDailyStatisticQuery(IStatelessUnitOfWork uow, DateTimePeriod date, string queueList, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			return session(uow).CreateSQLQuery("exec mart.raptor_queue_statistics_load @DateFrom=:DateFrom, @DateTo=:DateTo, @QueueList=:QueueList, @TimeZoneCode=:TimeZoneId, @MidnightBreakDifference=:MidnightBreakDifference")
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

		private IQuery createSkillIntervalStatisticQuery(IStatelessUnitOfWork uow, DateTimePeriod date, string timeZoneId, TimeSpan midnightBreakOffset)
		{
			return session(uow).CreateSQLQuery("exec mart.IntradaySkillStatistics @DateFrom=:DateFrom, @DateTo=:DateTo,  @TimeZoneCode=:TimeZoneId, @MidnightBreakDifference=:MidnightBreakDifference")
				.AddScalar("SkillId", NHibernateUtil.Guid)
				.AddScalar("SkillName", NHibernateUtil.String)
				.AddScalar("Interval", NHibernateUtil.DateTime)
				.AddScalar("StatAverageTaskTimeSeconds", NHibernateUtil.Double)
				.AddScalar("StatAverageAfterTaskTimeSeconds", NHibernateUtil.Double)
				.AddScalar("StatOfferedTasks", NHibernateUtil.Double)
				.AddScalar("StatAnsweredTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedShortTasks", NHibernateUtil.Double)
				.AddScalar("StatAbandonedTasksWithinSL", NHibernateUtil.Double)
				.AddScalar("StatOverflowOutTasks", NHibernateUtil.Double)
				.AddScalar("StatOverflowInTasks", NHibernateUtil.Double)
				.SetReadOnly(true)
				.SetString("DateFrom", date.StartDateTime.ToString(CultureInfo.InvariantCulture))
				.SetString("DateTo", date.EndDateTime.ToString(CultureInfo.InvariantCulture))
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
			return session(uow).CreateSQLQuery("exec mart.raptor_load_agent_count @skill=:skill,@start_date=:start_date, @end_date=:end_date")
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

		public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId, int adherenceId)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return session(uow).CreateSQLQuery(
					 "exec mart.raptor_adherence_report_load @date_from=:date_from, @time_zone_id=:time_zone_id, @person_code=:person_code, @agent_person_code=:agent_person_code, @language_id=:language_id, @adherence_id=:adherence_id")

					 .SetReadOnly(true)
					 .SetDateTime("date_from", dateTime)
					 .SetString("time_zone_id", timeZoneId)
					 .SetGuid("person_code", personCode)
					 .SetGuid("agent_person_code", agentPersonCode)
					 .SetInt32("language_id", languageId)
					 .SetInt32("adherence_id", adherenceId)
					 .List();
			}
		}

		public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return session(uow).CreateSQLQuery(
					 "exec mart.raptor_stat_agent @scenario_code=:scenario_code, @date_from=:date_from, @date_to=:date_to, @time_zone_code=:time_zone_code, @person_code=:person_code")

					 .SetReadOnly(true)
					 .SetGuid("scenario_code", scenarioCode)
					 .SetDateTime("date_from", startDate)
					 .SetDateTime("date_to", endDate)
					 .SetString("time_zone_code", timeZoneId)
					 .SetGuid("person_code", personCode)
					 .List();
			}
		}

		public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
		{
			using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return session(uow).CreateSQLQuery(
					 "exec mart.raptor_stat_agent_queue @date_from=:date_from, @date_to=:date_to, @time_zone_code=:time_zone_code, @person_code=:person_code")

					 .SetReadOnly(true)
					 .SetDateTime("date_from", startDate)
					 .SetDateTime("date_to", endDate)
					 .SetString("time_zone_code", timeZoneId)
					 .SetGuid("person_code", personCode)
					 .List();
			}
		}

		public IList LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold, Guid businessUnitId)
		{
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string sql =
				"exec [mart].[raptor_number_of_calls_per_agent_by_date] @threshold=:threshold, @time_zone_code=:timezoneCode, @local_date=:date, @business_unit_code=:businessUnitId";

				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.SetReadOnly(true)
					.SetInt32("threshold", answeredCallsThreshold)
					.SetString("timezoneCode", timezoneCode)
					.SetDateTime("date", date)
					.SetGuid("businessUnitId", businessUnitId)
					.List();
			}

		}

		public IList LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod, string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId)
		{
			var reportSetting = new AdherenceReportSetting
			{
				CalculationMethod = adherenceCalculationMethod
			};

			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string sql =
				"exec [mart].[raptor_adherence_per_agent_by_date] @threshold=:threshold, @time_zone_code=:timezoneCode, @local_date=:date, @adherence_id=:adherenceId, @business_unit_code=:businessUnitId";

				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.SetReadOnly(true)
					.SetDouble("threshold", adherenceThreshold.Value)
					.SetString("timezoneCode", timezoneCode)
					.SetDateTime("date", date)
					.SetInt32("adherenceId", reportSetting.AdherenceIdForReport())
					.SetGuid("businessUnitId", businessUnitId)
					.List();
			}
		}

		public IList LoadAgentsUnderThresholdForAHT(string timezoneCode, DateTime date, TimeSpan aHTThreshold, Guid businessUnitId)
		{
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string sql =
				"exec [mart].[raptor_AHT_per_agent_by_date] @threshold=:threshold, @time_zone_code=:timezoneCode, @local_date=:date, @business_unit_code=:businessUnitId";

				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.SetReadOnly(true)
					.SetDouble("threshold", aHTThreshold.TotalSeconds)
					.SetString("timezoneCode", timezoneCode)
					.SetDateTime("date", date)
					.SetGuid("businessUnitId", businessUnitId)
					.List();
			}
		}

		public IEnumerable<ForecastActualDifferNotification> ForecastActualDifferNotifications()
		{
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string sql =
				"exec [mart].[raptor_forecast_actual_differ_notifications]";

				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.AddScalar("Receiver", NHibernateUtil.String)
					.AddScalar("Subject", NHibernateUtil.String)
					.AddScalar("Body", NHibernateUtil.String)
					.SetReadOnly(true)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ForecastActualDifferNotification))).List<ForecastActualDifferNotification>();
			}
		}

		public IEnumerable<RunningEtlJob> GetRunningEtlJobs()
		{
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				const string sql = "exec [mart].[sys_etl_job_running_info_get]";

				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
					.AddScalar("computer_name", NHibernateUtil.String)
					.AddScalar("start_time", NHibernateUtil.DateTime)
					.AddScalar("job_name", NHibernateUtil.String)
					.AddScalar("is_started_by_service", NHibernateUtil.Boolean)
					.AddScalar("lock_until", NHibernateUtil.DateTime)
					.SetReadOnly(true)
					.List<object[]>()
					.Select(x =>
					  new RunningEtlJob
					  {
						  ComputerName = (string)x[0],
						  StartTime = (DateTime)x[1],
						  JobName = (string)x[2],
						  IsStartedByService = (bool)x[3],
						  LockUntil = (DateTime)x[4]
					  });
			}
		}

		private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
			return identity.DataSource.Statistic;
		}
	}

	public class StatisticPeriod
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
	//                                                                                         
	public class IntradayStatistics: IIntradayStatistics
	{
		public Guid SkillId { get; set; }
		public string SkillName { get; set; }
		public DateTime Interval { get; set; }
		public double StatAnsweredTasks { get; set; }
		public double StatOfferedTasks { get; set; }
		public double StatAbandonedTasks { get; set; }
		public int StatAbandonedShortTasks { get; set; }
		public double StatAbandonedTasksWithinSL { get; set; }
		public double StatOverflowOutTasks { get; set; }
		public double StatOverflowInTasks { get; set; }
		public double StatAverageTaskTimeSeconds { get; set; }
		public double StatAverageAfterTaskTimeSeconds { get; set; }
	}
}
