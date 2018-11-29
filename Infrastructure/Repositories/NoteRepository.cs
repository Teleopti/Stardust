using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class NoteRepository: Repository<INote>, INoteRepository 
    {
				public NoteRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
        
        public ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
        {
            var retList = new List<INote>();
            foreach (var personList in personCollection.Batch(400))
            {
                ICriteria crit = Session.CreateCriteria(typeof(Note))
                .Add(Restrictions.Between("NoteDate", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
                .Add(Restrictions.InG("Person", personList.ToArray()))
                .Add(Restrictions.Eq("Scenario", scenario))
                .AddOrder(Order.Asc("Person"))
                .AddOrder(Order.Asc("NoteDate"));
                retList.AddRange(crit.List<INote>());
            }

            return retList;
        }


        public INote LoadAggregate(Guid id)
        {
            return Get(id);
        }
    }
}
