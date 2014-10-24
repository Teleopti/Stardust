using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
    public class RtaRepository : IRtaRepository
    {
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
        public void AddOrUpdateActualAgentState(IActualAgentState actualAgentState)
        {
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                const string stringQuery = @"[RTA].[rta_addorupdate_actualagentstate] @PersonId=:personId,  @StateCode=:stateCode, @PlatformTypeId=:platform, 
					@State=:state, @StateId=:stateId, @Scheduled=:scheduled, @ScheduledId=:scheduledId, @StateStart=:stateStart, @ScheduledNext=:scheduledNext,  
					@ScheduledNextId=:scheduledNextId, @NextStart=:nextStart, @AlarmName=:alarmName, @AlarmId=:alarmId, @Color=:color, @AlarmStart=:alarmStart, 
					@StaffingEffect=:staffingEffect, @ReceivedTime=:receivedTime, @BatchId=:batchId, @OriginalDataSourceId=:originalDataSourceId";
                uow.Session().CreateSQLQuery(stringQuery)
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
