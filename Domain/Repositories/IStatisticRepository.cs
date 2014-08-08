using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Respository for Matrix stuff
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-23
    /// </remarks>
    public interface IStatisticRepository
    {
        /// <summary>
        /// Loads the specific dates.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period);

        /// <summary>
        /// Stub for loading matrix reports
        /// </summary>
        /// <returns></returns>
        ICollection<MatrixReportInfo> LoadReports();

        /// <summary>
        /// Stub for loading the queues from Matrix.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        ICollection<IQueueSource> LoadQueues();

        /// <summary>
        /// Loads the active agent count.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-19
        /// </remarks>
        ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period);

        /// <summary>
        /// Persists the fact queues.
        /// </summary>
        /// <param name="queueDataTable">The queue data table.</param>
        /// <remarks>
        /// Created by: ingemarg
        /// Created date: 2008-11-07
        /// </remarks>
        void PersistFactQueues(DataTable queueDataTable);

        /// <summary>
        /// Runs the "raptor_v_stg_queue_delete.sql proc"
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-02-04
        /// </remarks>
        void DeleteStgQueues();

        /// <summary>
        /// Runs the "raptor_fact_queue_load.sql proc"
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-02-04
        /// </remarks>
        void LoadFactQueues();

        /// <summary>
        /// Runs the "raptor_dim_queue_load.sql proc"
        /// </summary>
        /// <remarks>
        /// Created by: ingemarg
        /// Created date: 2009-02-04
        /// </remarks>
        void LoadDimQueues();

        IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode,
                                 Guid agentPersonCode, int languageId, int adherenceId);

        /// <summary>
        /// Loads the agent stat.
        /// </summary>
        /// <param name="scenarioCode">The scenario code.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <param name="personCode">The person code.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-01
        /// </remarks>
        IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);

        /// <summary>
        /// Loads the agent queue stat.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <param name="personCode">The person code.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-01
        /// </remarks>
        IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode);


        IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons);

        IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids);

        IActualAgentState LoadOneActualAgentState(Guid value);

        void AddOrUpdateActualAgentState(IActualAgentState actualAgentState);

		IEnumerable<Guid> LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date);
		IEnumerable<Guid> LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod, string timezoneCode, DateTime date);
		IEnumerable<Guid> LoadAgentsUnderThresholdForAHT(string timezoneCode, DateTime date);
		IEnumerable<ISimpleTimeZone> LoadAllTimeZones(IStatelessUnitOfWork uow);
    }
}