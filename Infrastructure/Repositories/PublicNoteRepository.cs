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
    public class PublicNoteRepository: Repository<IPublicNote>, IPublicNoteRepository 
    {
        public PublicNoteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
       
        public PublicNoteRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
		
        public PublicNoteRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public IList<IPublicNote> Find(DateTimePeriod period, IScenario scenario)
        {
            ICriteria crit = Session.CreateCriteria(typeof(PublicNote))
                .Add(Restrictions.Between("NoteDate", period.StartDateTime, period.EndDateTime))
                .Add(Restrictions.Eq("Scenario", scenario));
            IList<IPublicNote> retList = crit.List<IPublicNote>();
            return retList;
        }

        public ICollection<IPublicNote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
        {
            var retList = new List<IPublicNote>();
            foreach (var personList in personCollection.Batch(400))
            {
                ICriteria crit = Session.CreateCriteria(typeof(PublicNote))
                .Add(Restrictions.Between("NoteDate", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
                .Add(Restrictions.In("Person", new List<IPerson>(personList)))
                .Add(Restrictions.Eq("Scenario", scenario))
                .AddOrder(Order.Asc("Person"))
                .AddOrder(Order.Asc("NoteDate"));
                retList.AddRange(crit.List<IPublicNote>());
            }
            
            return retList;
        }

        public IPublicNote Find(DateOnly dateOnly, IPerson person, IScenario scenario)
        {
            return Session.CreateCriteria(typeof (PublicNote))
                .Add(Restrictions.Eq("NoteDate", dateOnly))
                .Add(Restrictions.Eq("Person", person))
                .Add(Restrictions.Eq("Scenario", scenario))
                .UniqueResult<IPublicNote>();
        }

        public IPublicNote LoadAggregate(Guid id)
        {
            return Get(id);
        }
    }
}
