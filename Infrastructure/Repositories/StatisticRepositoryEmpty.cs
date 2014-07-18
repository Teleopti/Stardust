using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for statistic when no matrix db is defined
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-29
    /// </remarks>
    public class StatisticRepositoryEmpty : IStatisticRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticRepositoryEmpty"/> class.
        /// Only to be created from the factory.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-29
        /// </remarks>
        internal StatisticRepositoryEmpty() { }

        /// <summary>
        /// Loads the specific dates.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources,
                                                            DateTimePeriod period)
        {
            return new List<IStatisticTask>();
        }

        /// <summary>
        /// Stub for loading matrix reports. 
        /// Note: It will return null, instead of an empty list.
        /// </summary>
        /// <returns></returns>
        public ICollection<MatrixReportInfo> LoadReports()
        {
            return null;
        }

        /// <summary>
        /// Stub for loading the queues from Matrix.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        public ICollection<IQueueSource> LoadQueues()
        {
            return new List<IQueueSource>();
        }

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
        public ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period)
        {
            return new List<IActiveAgentCount>();
        }

        public void PersistFactQueues(DataTable queueDataTable)
        {
        }

        //public int PersistDimQueue(int code, string name)
        //{
        //    return -1;
        //}

        public void DeleteStgQueues()
        {

        }

        public void LoadFactQueues()
        {

        }

        public void LoadDimQueues()
        {

        }

        public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId, int adherenceId)
        {
            return new List<object>();
        }

        #region IStatisticRepository Members

        public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
        {
            return new List<object>();
        }
        public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
        {
            return new List<object>();
        }

        public IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons)
        {
            return new List<IActualAgentState>();
        }

	    public IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids)
	    {
			return new List<IActualAgentState>();
	    }

	    public IActualAgentState LoadOneActualAgentState(Guid value)
        {
            return new ActualAgentState();
        }

        public void AddOrUpdateActualAgentState(IActualAgentState actualAgentState)
        {
        }

		public IEnumerable<Tuple<Guid, int>> LoadAgentsOverThresholdForAnsweredCalls(IUnitOfWork uow)
	    {
		    return new List<Tuple<Guid, int>>();
		}

		public IEnumerable<Tuple<Guid, int>> LoadAgentsOverThresholdForAdherence(IUnitOfWork uow)
		{
			return new List<Tuple<Guid, int>>();
		}

		public IEnumerable<Tuple<Guid, int>> LoadAgentsUnderThresholdForAHT(IUnitOfWork uow)
		{
			return new List<Tuple<Guid, int>>();
		}

		public ICollection<Guid> PersonIdsWithExternalLogOn(Guid businessUnitId)
        {
            return new List<Guid>();
        }

        #endregion
    }
}
