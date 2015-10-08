using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class NoteRepository: Repository<INote>, INoteRepository 
    {
#pragma warning disable 618
        public NoteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public NoteRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public IList<INote> Find(DateTimePeriod period, IScenario scenario)
        {
            ICriteria crit = Session.CreateCriteria(typeof(Note))
                .Add(Restrictions.Between("NoteDate", period.StartDateTime, period.EndDateTime))
                .Add(Restrictions.Eq("Scenario", scenario));
            IList<INote> retList = crit.List<INote>();
            return retList;
        }
        
        public ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
        {
            var retList = new List<INote>();
            foreach (var personList in personCollection.Batch(400))
            {
                ICriteria crit = Session.CreateCriteria(typeof(Note))
                .Add(Restrictions.Between("NoteDate", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
                .Add(Restrictions.In("Person", new List<IPerson>(personList)))
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
