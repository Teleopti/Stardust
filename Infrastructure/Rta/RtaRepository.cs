using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
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
        public IList<AgentStateReadModel> LoadActualAgentState(IEnumerable<IPerson> persons)
        {
            var guids = persons.Select(person => person.Id.GetValueOrDefault()).ToList();
	        return LoadLastAgentState(guids);
        }

        public IList<AgentStateReadModel> LoadLastAgentState(IEnumerable<Guid> personGuids)
        {
            using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                var ret = new List<AgentStateReadModel>();
				foreach (var personList in personGuids.Batch(400))
				{
					ret.AddRange(((NHibernateStatelessUnitOfWork) uow).Session.CreateSQLQuery(
						"SELECT * FROM RTA.ActualAgentState WITH (NOLOCK) WHERE PersonId IN(:persons)")
						.SetParameterList("persons", personList)
						.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
						.SetReadOnly(true)
						.List<AgentStateReadModel>());
				}
                return ret;
            }
        }

	    public IList<AgentStateReadModel> LoadTeamAgentStates(Guid teamId)
	    {
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"SELECT * FROM RTA.ActualAgentState WITH (NOLOCK) WHERE TeamId = :teamId")
					.SetParameter("teamId", teamId)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}  
	    }

	    private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return identity.DataSource.Statistic;
        }
    }
}
