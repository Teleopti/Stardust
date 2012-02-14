using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps an Person Work Rule / Period to an PersonContract
    /// </summary>
    public class AgentContractMapper : Mapper<PersonContract, global::Domain.Agent>
    {
        private readonly DateTime _baseDate;


        /// <summary>
        /// Initializes a new instance of the <see cref="AgentContractMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentContractMapper(MappedObjectPair mappedObjectPair, 
                                    TimeZoneInfo timeZone,
                                    DateTime date) : base(mappedObjectPair, timeZone)
        {
            _baseDate = date;
        }


        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override PersonContract Map(global::Domain.Agent oldEntity)
        {
            PersonContract agContract = null;

            if (oldEntity.PeriodCollection == null ||
                oldEntity.PeriodCollection.Count < 1) return agContract;

            global::Domain.AgentWorkrule agentWorkRule = oldEntity.Workrule(_baseDate);
            if (agentWorkRule == null) return agContract;

            global::Domain.WorktimeType workTimeType = agentWorkRule.TypeOfWorkTime;

            IEnumerator<global::Domain.AgentPeriod> enumeratorAgentPeriod = oldEntity.PeriodCollection.GetEnumerator();
            enumeratorAgentPeriod.MoveNext();
            global::Domain.AgentPeriod agentPeriod = enumeratorAgentPeriod.Current;
            agContract =
                new PersonContract(MappedObjectPair.Agent.GetPaired(oldEntity),
                                  TimeZoneInfo.ConvertTimeToUtc(agentPeriod.Period.StartDate, TimeZone),
                                  MappedObjectPair.Contract.GetPaired(workTimeType),
                                  MappedObjectPair.PartTimePercentage.GetPaired(workTimeType),
                                  MappedObjectPair.ContractSchedule.GetPaired(workTimeType));

            return agContract;
        }
    }
}