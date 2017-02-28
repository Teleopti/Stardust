using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PersonAbsenceAccountRepository : Repository<IPersonAbsenceAccount>,
                                                    IPersonAbsenceAccountRepository
    {
        public PersonAbsenceAccountRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

	    public PersonAbsenceAccountRepository(ICurrentUnitOfWork currentUnitOfWork)
		    : base(currentUnitOfWork)
	    {
	    }

	    public IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts()
        {
            var ret = new dic(new Dictionary<IPerson, IPersonAccountCollection>());

            var result = Session.CreateCriteria(typeof (PersonAbsenceAccount))
						.SetFetchMode("accountCollection", FetchMode.Join)
                        .SetFetchMode("Person",FetchMode.Join)
                        .SetFetchMode("Absence",FetchMode.Join)
                        .SetResultTransformer(Transformers.DistinctRootEntity)
                        .List<IPersonAbsenceAccount>();
            foreach (var paAcc in result)
            {
                ret[paAcc.Person].Add(paAcc);
            }

            return ret;
        }

        public IDictionary<IPerson, IPersonAccountCollection> FindByUsers(IEnumerable<IPerson> persons)
        {
            var ret = new dic(new Dictionary<IPerson, IPersonAccountCollection>());

            foreach (var personBatch in persons.Batch(400))
            {
                var result = Session.CreateCriteria(typeof(PersonAbsenceAccount))
                       .SetFetchMode("accountCollection", FetchMode.Join)
                       .SetFetchMode("Person", FetchMode.Join)
                       .SetFetchMode("Absence", FetchMode.Join)
                       .Add(Restrictions.InG("Person", personBatch.ToArray()))
                       .SetResultTransformer(Transformers.DistinctRootEntity)
                       .List<IPersonAbsenceAccount>();

                foreach (var paAcc in result)
                {
                    ret[paAcc.Person].Add(paAcc);
                }    
            }
           

            return ret;
        }

        public IPersonAccountCollection Find(IPerson person)
        {
            var result = Session.CreateCriteria(typeof(PersonAbsenceAccount))
						.SetFetchMode("accountCollection", FetchMode.Join)
                        .Add(Restrictions.Eq("Person",person))
                        .SetResultTransformer(Transformers.DistinctRootEntity)
                        .List<IPersonAbsenceAccount>();
            var personAccountCollection = new PersonAccountCollection(person);
            foreach (var personAbsenceAccount in result)
            {
                personAccountCollection.Add(personAbsenceAccount);
            }
            return personAccountCollection;
        }


        private class dic : AbstractDictionary<IPerson, IPersonAccountCollection>
        {
            public dic(IDictionary<IPerson, IPersonAccountCollection> dic)
                : base(dic)
            {
            }

            public override IPersonAccountCollection this[IPerson key]
            {
                get
                {
                    //for now
                    IPersonAccountCollection ret;
                    if (!TryGetValue(key, out ret))
                    {
                        ret = new PersonAccountCollection(key);
                        base[key] = ret;
                    }
                    return ret;
                }
                set { base[key] = value; }
            }
        }
    }
}