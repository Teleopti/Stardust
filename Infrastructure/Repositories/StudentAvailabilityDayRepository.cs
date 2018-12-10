using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for person rotations
    /// </summary>
    public class StudentAvailabilityDayRepository : Repository<IStudentAvailabilityDay>, IStudentAvailabilityDayRepository
    {
        public StudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public StudentAvailabilityDayRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public IList<IStudentAvailabilityDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
        {
            var result = new List<IStudentAvailabilityDay>();

            if (!persons.Any()) return result;

            foreach (var personList in persons.Batch(400))
            {
                ICriteria crit = FilterByPeriod(period)
                .Fetch("RestrictionCollection")
                .Add(personCriterion(personList.ToArray()))
                .SetResultTransformer(Transformers.DistinctRootEntity);
                result.AddRange(crit.List<IStudentAvailabilityDay>());
            }

            InitializeStudentDays(result);
            return result;
        }

        public IList<IStudentAvailabilityDay> Find(DateOnly dateOnly, IPerson person)
        {
            ICriteria crit = Session.CreateCriteria(typeof(StudentAvailabilityDay))
                .Add(Restrictions.Eq("RestrictionDate", dateOnly))
                .Add(personCriterion(person))
				.AddOrder(Order.Desc("UpdatedOn"))
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .Fetch("Restriction");
            IList<IStudentAvailabilityDay> retList = crit.List<IStudentAvailabilityDay>();

            InitializeStudentDays(retList);
            return retList;
        }

		public IList<IStudentAvailabilityDay> FindNewerThan(DateTime newerThan)
		{
			var crit = Session.CreateCriteria(typeof(StudentAvailabilityDay))
				.Add(Restrictions.Gt("UpdatedOn", newerThan))
				.Fetch("RestrictionCollection");
			var retList = crit.List<IStudentAvailabilityDay>();

			InitializeStudentDays(retList);
			return retList;
		}

        private ICriteria FilterByPeriod(DateOnlyPeriod period)
        {
            return Session.CreateCriteria(typeof(StudentAvailabilityDay))

                .Add(Restrictions.Between("RestrictionDate", period.StartDate, period.EndDate))
                .AddOrder(Order.Asc("Person"))
                .AddOrder(Order.Asc("RestrictionDate"))
				.AddOrder(Order.Desc("UpdatedOn"));
        }

        private static void InitializeStudentDays(IEnumerable<IStudentAvailabilityDay> studentDays)
        {
            foreach (IStudentAvailabilityDay day in studentDays)
            {
                if (!LazyLoadingManager.IsInitialized(day.Person))
                    LazyLoadingManager.Initialize(day.Person);
            }
        }

        private static ICriterion personCriterion(params IPerson[] personCollection)
        {
            ICriterion ret;
            if (personCollection.Length > 1)
                ret = Restrictions.InG("Person", personCollection);
            else
                ret = Restrictions.Eq("Person", personCollection[0]);
            return ret;
        }

        public IStudentAvailabilityDay LoadAggregate(Guid id)
        {
            IStudentAvailabilityDay ret = Session.CreateCriteria(typeof(StudentAvailabilityDay))
                .Fetch("RestrictionCollection")
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .Add(Restrictions.Eq("Id", id))
                .UniqueResult<IStudentAvailabilityDay>();

            return ret;
        }
    }
}