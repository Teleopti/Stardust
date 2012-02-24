using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class WebReportRepository : IWebReportRepository
	{
		private readonly IUnitOfWorkFactory _statisticUowFactory;

		public WebReportRepository(IDataSource dataSource)
		{
			_statisticUowFactory = dataSource.Statistic;
		}

		#region IWebReportRepository Members

		public IEnumerable<ReportControlSkillGet> ReportControlSkillGet(Guid reportId, Guid personCode, int languageId,
		                                                                   Guid businessUnitCode)
		{
			using (IStatelessUnitOfWork uow = _statisticUowFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return
					session(uow).CreateSQLQuery(
						"exec mart.report_control_skill_get @report_id=:report_id, @person_code=:person_code,@language_id=:language_id, @bu_id=:bu_id")
						.AddScalar("Id", NHibernateUtil.Int32)
						.AddScalar("Name", NHibernateUtil.String)
						.SetReadOnly(true)
						.SetGuid("report_id", reportId)
						.SetGuid("person_code", personCode)
						.SetInt32("language_id", languageId)
						.SetGuid("bu_id", businessUnitCode)
						.SetResultTransformer(Transformers.AliasToBean(typeof (ReportControlSkillGet)))
						.List<ReportControlSkillGet>();
			}
		}

		public ReportMobileReportInit ReportMobileReportInit(Guid personCode, int languageId, Guid businessUnitCode,
		                                                        string skillSet, string timeZoneCode)
		{
			using (IStatelessUnitOfWork uow = _statisticUowFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return
					session(uow).CreateSQLQuery(
						"exec [mart].[report_mobilereport_init] @person_code=:person_code, @language_id=:language_id, @bu_code=:bu_code, @skill_set=:skill_set, @time_zone_code=:time_zone_code")
						.AddScalar("Scenario", NHibernateUtil.Int32)
						.AddScalar("TimeZone", NHibernateUtil.Int32)
						.AddScalar("IntervalFrom", NHibernateUtil.Int32)
						.AddScalar("IntervalTo", NHibernateUtil.Int32)
						.AddScalar("ServiceLevelCalculationId", NHibernateUtil.Int32)
						.AddScalar("WorkloadSet", NHibernateUtil.String)
						.AddScalar("SkillSet", NHibernateUtil.String)
						.SetReadOnly(true)
						.SetGuid("person_code", personCode)
						.SetInt32("language_id", languageId)
						.SetGuid("bu_code", businessUnitCode)
						.SetString("skill_set", skillSet)
						.SetString("time_zone_code", timeZoneCode)
						.SetResultTransformer(Transformers.AliasToBean(typeof (ReportMobileReportInit)))
						.List<ReportMobileReportInit>().First();
			}
		}

		public IEnumerable<ReportDataForecastVersusActualWorkload> ReportDataForecastVersusActualWorkload(int scenarioId,
		                                                                                                  string skillSet,
		                                                                                                  string workloadSet,
		                                                                                                  int intervalType,
		                                                                                                  DateTime dateFrom,
		                                                                                                  DateTime dateTo,
		                                                                                                  int intervalFrom,
		                                                                                                  int intervalTo,
		                                                                                                  int timeZoneId,
		                                                                                                  Guid personCode,
		                                                                                                  Guid reportId,
		                                                                                                  int languageId,
		                                                                                                  Guid
		                                                                                                  	businessUnitCode)
		{
			ConstructorInfo constructorInfo =
				typeof (ReportDataForecastVersusActualWorkload).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
				                                                               CallingConventions.HasThis,
				                                                               new[]
				                                                               	{
				                                                               		typeof (string), typeof (decimal), typeof (decimal)
				                                                               	},
				                                                               null);

			using (IStatelessUnitOfWork uow = _statisticUowFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return
					session(uow).CreateSQLQuery(
						"exec [mart].[report_data_forecast_vs_actual_workload] " +
						"@scenario_id=:scenario_id, @skill_set=:skill_set, @workload_set=:workload_set, @interval_type=:interval_type, " +
						"@date_from=:date_from, @date_to=:date_to, @interval_from=:interval_from, @interval_to=:interval_to," +
						"@time_zone_id=:time_zone_id,  @person_code=:person_code, @report_id=:report_id, @language_id=:language_id, @business_unit_code=:bu_code")
						.AddScalar("period", NHibernateUtil.String)
						.AddScalar("forecasted_calls", NHibernateUtil.Decimal)
						.AddScalar("offered_calls", NHibernateUtil.Decimal)
						.SetReadOnly(true).SetInt32("scenario_id", scenarioId)
						.SetString("skill_set", skillSet)
						.SetString("workload_set", workloadSet)
						.SetInt32("interval_type", intervalType)
						.SetDateTime("date_from", dateFrom)
						.SetDateTime("date_to", dateTo)
						.SetInt32("interval_from", intervalFrom)
						.SetInt32("interval_to", intervalTo)
						.SetInt32("time_zone_id", timeZoneId)
						.SetGuid("person_code", personCode)
						.SetGuid("report_id", reportId)
						.SetInt32("language_id", languageId)
						.SetGuid("bu_code", businessUnitCode)
						.SetResultTransformer(Transformers.AliasToBeanConstructor(constructorInfo))
						.List<ReportDataForecastVersusActualWorkload>();
			}
		}


		public IEnumerable<ReportDataQueueStatAbandoned> ReportDataQueueStatAbandoned(int scenarioId,
		                                                                              string skillSet,
		                                                                              string workloadSet,
		                                                                              int intervalType,
		                                                                              DateTime dateFrom,
		                                                                              DateTime dateTo,
		                                                                              int intervalFrom,
		                                                                              int intervalTo,
		                                                                              int timeZoneId,
		                                                                              Guid personCode,
																					  Guid reportId,
		                                                                              int languageId,
		                                                                              Guid businessUnitCode)
		{
			ConstructorInfo constructorInfo =
				typeof (ReportDataQueueStatAbandoned).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
				                                                     CallingConventions.HasThis,
				                                                     new[] {typeof (string), typeof (decimal), typeof (decimal)},
				                                                     null);

			using (IStatelessUnitOfWork uow = _statisticUowFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return
					session(uow).CreateSQLQuery(
						"exec [mart].[report_data_queue_stat_abnd] " +
						"@skill_set=:skill_set, @workload_set=:workload_set, @interval_type=:interval_type, " +
						"@date_from=:date_from, @date_to=:date_to, @interval_from=:interval_from, @interval_to=:interval_to," +
						"@time_zone_id=:time_zone_id,  @person_code=:person_code, @report_id=:report_id, @language_id=:language_id, @business_unit_code=:bu_code")
						.AddScalar("period", NHibernateUtil.String)
						.AddScalar("calls_answ", NHibernateUtil.Decimal)
						.AddScalar("calls_abnd", NHibernateUtil.Decimal)
						.SetReadOnly(true)
						.SetString("skill_set", skillSet)
						.SetString("workload_set", workloadSet)
						.SetInt32("interval_type", intervalType)
						.SetDateTime("date_from", dateFrom)
						.SetDateTime("date_to", dateTo)
						.SetInt32("interval_from", intervalFrom)
						.SetInt32("interval_to", intervalTo)
						.SetInt32("time_zone_id", timeZoneId)
						.SetGuid("person_code", personCode)
						.SetGuid("report_id", reportId)
						.SetInt32("language_id", languageId)
						.SetGuid("bu_code", businessUnitCode)
						.SetResultTransformer(Transformers.AliasToBeanConstructor(constructorInfo))
						.List<ReportDataQueueStatAbandoned>();
			}
		}

		public IEnumerable<ReportDataServiceLevelAgentsReady> ReportDataServiceLevelAgentsReady(string skillSet,
		                                                                                        string workloadSet,
		                                                                                        int intervalType,
		                                                                                        DateTime dateFrom,
		                                                                                        DateTime dateTo,
		                                                                                        int intervalFrom,
		                                                                                        int intervalTo,
		                                                                                        int serviceLevelCalculationId,
		                                                                                        int timeZoneId,
		                                                                                        Guid personCode,
		                                                                                        Guid reportId,
		                                                                                        int languageId,
		                                                                                        Guid businessUnitCode)
		{
			ConstructorInfo constructorInfo =
				typeof (ReportDataServiceLevelAgentsReady).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
				                                                          CallingConventions.HasThis,
				                                                          new[] { typeof(string), typeof(decimal), typeof(decimal), typeof(decimal) },
				                                                          null);

			using (IStatelessUnitOfWork uow = _statisticUowFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return
					session(uow).CreateSQLQuery(
						"exec [mart].[report_data_service_level_agents_ready] " +
						"@skill_set=:skill_set, @workload_set=:workload_set, @interval_type=:interval_type, " +
						"@date_from=:date_from, @date_to=:date_to, @interval_from=:interval_from, @interval_to=:interval_to, @sl_calc_id=:sl_calc_id," +
						"@time_zone_id=:time_zone_id,  @person_code=:person_code, @report_id=:report_id, @language_id=:language_id, @business_unit_code=:bu_code")
						.AddScalar("period", NHibernateUtil.String)
						.AddScalar("scheduled_agents_ready", NHibernateUtil.Decimal)
						.AddScalar("agents_ready", NHibernateUtil.Decimal)
						.AddScalar("service_level", NHibernateUtil.Decimal)
						.SetReadOnly(true)
						.SetString("skill_set", skillSet)
						.SetString("workload_set", workloadSet)
						.SetInt32("interval_type", intervalType)
						.SetDateTime("date_from", dateFrom)
						.SetDateTime("date_to", dateTo)
						.SetInt32("interval_from", intervalFrom)
						.SetInt32("interval_to", intervalTo)
						.SetInt32("sl_calc_id", serviceLevelCalculationId)
						.SetInt32("time_zone_id", timeZoneId)
						.SetGuid("person_code", personCode)
						.SetGuid("report_id", reportId)
						.SetInt32("language_id", languageId)
						.SetGuid("bu_code", businessUnitCode)
						.SetResultTransformer(Transformers.AliasToBeanConstructor(constructorInfo))
						.List<ReportDataServiceLevelAgentsReady>();
			}
		}

		#endregion

		private static IStatelessSession session(IStatelessUnitOfWork uow)
		{
			return ((NHibernateStatelessUnitOfWork) uow).Session;
		}
	}
}