﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public interface IOvertimeAvailabilityRepository : IRepository<IOvertimeAvailability>, ILoadAggregateById<IOvertimeAvailability >
    {
        IList<IOvertimeAvailability> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
        IList<IOvertimeAvailability> Find(DateOnly dateOnly, IPerson person);
    }

    public class OvertimeAvailabilityRepository : Repository<IOvertimeAvailability>, IOvertimeAvailabilityRepository
    {
        
        public OvertimeAvailabilityRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public OvertimeAvailabilityRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        public OvertimeAvailabilityRepository(ICurrentUnitOfWork currentUnitOfWork)
            : base(currentUnitOfWork)
        {
		    
        }

        public IList<IOvertimeAvailability > Find(DateOnly dateOnly, IPerson person)
        {
            ICriteria crit = Session.CreateCriteria(typeof(OvertimeAvailability))
                                    .Add(Restrictions.Eq("DateOfOvertime", dateOnly))
                                    .Add(Restrictions.Eq("Person", person))
                                    .SetResultTransformer(Transformers.DistinctRootEntity)
                                    .SetFetchMode("Restriction", FetchMode.Join);
            IList<IOvertimeAvailability> retList = crit.List<IOvertimeAvailability>();

            InitializeStudentDays(retList);
            return retList;
        }

        public IList<IOvertimeAvailability > Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
        {
            var result = new List<IOvertimeAvailability >();

            if (!persons.Any()) return result;

            foreach (var personList in persons.Batch(400))
            {
                ICriteria crit = FilterByPeriod(period)
                    .Add(personCriterion(personList.ToArray()))
                    .SetResultTransformer(Transformers.DistinctRootEntity);
                result.AddRange(crit.List<IOvertimeAvailability>());
            }

            InitializeStudentDays(result);
            return result;
        }

        private ICriteria FilterByPeriod(DateOnlyPeriod period)
        {
            return Session.CreateCriteria(typeof(OvertimeAvailability))

                          .Add(Restrictions.Between("DateOfOvertime", period.StartDate, period.EndDate))
                          .AddOrder(Order.Asc("Person"))
                          .AddOrder(Order.Asc("DateOfOvertime"));
        }

        private static void InitializeStudentDays(IEnumerable<IOvertimeAvailability> overtimeDays)
        {
            foreach (IOvertimeAvailability day in overtimeDays)
            {
                if (!LazyLoadingManager.IsInitialized(day.Person))
                    LazyLoadingManager.Initialize(day.Person);
            }
        }

        private static ICriterion personCriterion(IList<IPerson> personCollection)
        {
            ICriterion ret;
            if (personCollection.Count > 1)
                ret = Restrictions.InG("Person", personCollection);
            else
                ret = Restrictions.Eq("Person", personCollection[0]);
            return ret;
        }

        public IOvertimeAvailability LoadAggregate(Guid id)
        {
            IOvertimeAvailability ret = Session.CreateCriteria(typeof(OvertimeAvailability))
                                                 .SetResultTransformer(Transformers.DistinctRootEntity)
                                                 .Add(Restrictions.Eq("Id", id))
                                                 .UniqueResult<IOvertimeAvailability>();

            return ret;
        }

        
    }
}