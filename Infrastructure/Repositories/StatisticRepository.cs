using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
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

        public IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons)
        {
            var guids = persons.Select(person => person.Id.GetValueOrDefault()).ToList();
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                var ret = new List<IActualAgentState>();
                foreach (var personList in guids.Batch(400))
                {
                    ret.AddRange(((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
                        "SELECT * FROM RTA.ActualAgentState WHERE PersonId IN(:persons)")
                        .SetParameterList("persons", personList)
                        .SetResultTransformer(Transformers.AliasToBean(typeof(ActualAgentState)))
                        .SetReadOnly(true)
                        .List<IActualAgentState>());
                }
                return ret;
            }
        }

        public IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids)
        {
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                var ret = new List<IActualAgentState>();
				foreach (var personList in personGuids.Batch(400))
				{
					ret.AddRange(((NHibernateStatelessUnitOfWork) uow).Session.CreateSQLQuery(
						"SELECT * FROM RTA.ActualAgentState WHERE PersonId IN(:persons)")
						.SetParameterList("persons", personList)
						.SetResultTransformer(Transformers.AliasToBean(typeof (ActualAgentState)))
						.SetReadOnly(true)
						.List<IActualAgentState>().GroupBy(x => x.PersonId, (key, group) => group.First()));
				}
                return ret;
            }
        }

        public IActualAgentState LoadOneActualAgentState(Guid value)
        {
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
                    "SELECT * FROM RTA.ActualAgentState WHERE PersonId =:value")
                    .SetGuid("value", value)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(ActualAgentState)))
                    .List<IActualAgentState>()
                    .FirstOrDefault();
            }
        }

		public IEnumerable<Tuple<Guid, int>> LoadAgentsOverThresholdForAnsweredCalls(IUnitOfWork uow)
	    {
			throw new NotImplementedException();
	    }

	    public IEnumerable<Tuple<Guid, int>> LoadAgentsOverThresholdForAdherence(IUnitOfWork uow)
	    {
		    throw new NotImplementedException();
	    }

	    public IEnumerable<Tuple<Guid, int>> LoadAgentsUnderThresholdForAHT(IUnitOfWork uow)
	    {
		    throw new NotImplementedException();
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddOrUpdateActualAgentState(IActualAgentState actualAgentState)
        {
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                const string stringQuery = @"[RTA].[rta_addorupdate_actualagentstate] @PersonId=:personId,  @StateCode=:stateCode, @PlatformTypeId=:platform, 
					@State=:state, @StateId=:stateId, @Scheduled=:scheduled, @ScheduledId=:scheduledId, @StateStart=:stateStart, @ScheduledNext=:scheduledNext,  
					@ScheduledNextId=:scheduledNextId, @NextStart=:nextStart, @AlarmName=:alarmName, @AlarmId=:alarmId, @Color=:color, @AlarmStart=:alarmStart, 
					@StaffingEffect=:staffingEffect, @ReceivedTime=:receivedTime, @BatchId=:batchId, @OriginalDataSourceId=:originalDataSourceId";
                session(uow).CreateSQLQuery(stringQuery)
                    .SetGuid("personId", actualAgentState.PersonId)
                    .SetString("stateCode", actualAgentState.StateCode)
                    .SetGuid("platform", actualAgentState.PlatformTypeId)
                    .SetString("state", actualAgentState.State)
                    .SetGuid("stateId", actualAgentState.StateId)
                    .SetString("scheduled", actualAgentState.Scheduled)
                    .SetGuid("scheduledId", actualAgentState.ScheduledId)
                    .SetDateTime("stateStart", actualAgentState.StateStart)
                    .SetString("scheduledNext", actualAgentState.ScheduledNext)
                    .SetGuid("scheduledNextId", actualAgentState.ScheduledNextId)
                    .SetDateTime("nextStart", actualAgentState.NextStart)
                    .SetString("alarmName", actualAgentState.AlarmName)
                    .SetGuid("alarmId", actualAgentState.AlarmId)
                    .SetInt32("color", actualAgentState.Color)
                    .SetDateTime("alarmStart", actualAgentState.AlarmStart)
                    .SetDouble("staffingEffect", actualAgentState.StaffingEffect)
					.SetDateTime("receivedTime", actualAgentState.ReceivedTime)
					.SetParameter("batchId", actualAgentState.BatchId)
					.SetString("originalDataSourceId", actualAgentState.OriginalDataSourceId)
                    .SetTimeout(300)
                    .ExecuteUpdate();
            }
        }

        private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return identity.DataSource.Statistic;
        }

    }


}
