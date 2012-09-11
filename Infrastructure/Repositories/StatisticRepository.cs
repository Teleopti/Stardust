﻿using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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

            string queueList = buildStringQueueList(sources);
            
            using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                IQuery query = createQuery(uow, period, queueList);
                query.SetTimeout(1200);
                return query.List<IStatisticTask>();
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

        public ICollection<IExternalAgentState> LoadRtaAgentStates(DateTimePeriod period, IList<ExternalLogOnPerson> externalLogOnPersons)
        {
            using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                IQuery query = createExternalAgentStateQuery(uow, period, externalLogOnPersons);
                return query.List<IExternalAgentState>();
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

        private static IQuery createExternalAgentStateQuery(IStatelessUnitOfWork uow, DateTimePeriod period, IList<ExternalLogOnPerson> externalLogOnPersons)
        {
            StringBuilder externalLogOnList = new StringBuilder();
            StringBuilder externalLogOnLogObjectIdList = new StringBuilder();
            foreach (ExternalLogOnPerson externalLogOnPerson in externalLogOnPersons)
            {
                if (externalLogOnList.Length>0)
                {
                    externalLogOnList.Append(",");
                    externalLogOnLogObjectIdList.Append(",");
                }
                externalLogOnList.Append(externalLogOnPerson.ExternalLogOn);
                externalLogOnLogObjectIdList.Append(externalLogOnPerson.DataSourceId);
            }

            return session(uow).CreateSQLQuery("exec RTA.rta_load_agentstate @start_date=:start_date, @end_date=:end_date, @externallogonlist=:externallogonlist")
                .AddScalar("ExternalLogOn", NHibernateUtil.String)
                .AddScalar("StateCode", NHibernateUtil.String)
                .AddScalar("Timestamp", NHibernateUtil.DateTime)
                .AddScalar("TimeInState", NHibernateUtil.TimeSpan)
                .AddScalar("PlatformTypeId", NHibernateUtil.Guid)
                .AddScalar("DataSourceId",NHibernateUtil.Int32)
                .AddScalar("BatchId",NHibernateUtil.DateTime)
                .AddScalar("IsSnapshot",NHibernateUtil.Boolean)
                .SetReadOnly(true)
                .SetDateTime("start_date", period.StartDateTime)
                .SetDateTime("end_date", period.EndDateTime)
                .SetParameter("externallogonlist", externalLogOnList.ToString(), NHibernateUtil.StringClob)
                .SetResultTransformer(Transformers.AliasToBean(typeof(ExternalAgentState)));
        }

        private static string buildStringQueueList(IEnumerable<IQueueSource> sources)
        {
            return String.Join(",", sources.Select(s => s
                .QueueMartId.ToString(CultureInfo.InvariantCulture)).ToArray());
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
                .AddScalar("StatAverageHandleTimeSeconds", NHibernateUtil.Int32)
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
                .SetGuid("skill", skill.Id.Value)
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

        public IList LoadAdherenceData(DateTime dateTimeFrom, DateTime dateTimeTo, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId, int adherenceId)
        {
            using (IStatelessUnitOfWork uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return session(uow).CreateSQLQuery(
					"exec mart.raptor_adherence_report_load @date_from=:date_from, @date_to=:date_to, @time_zone_id=:time_zone_id, @person_code=:person_code, @agent_person_code=:agent_person_code, @language_id=:language_id, @adherence_id=:adherence_id")

                    .SetReadOnly(true)
                    .SetDateTime("date_from", dateTimeFrom)
					.SetDateTime("date_to", dateTimeTo)
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


        private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return identity.DataSource.Statistic;
        }
    }
}
