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

        private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return identity.DataSource.Statistic;
        }
    }
}
