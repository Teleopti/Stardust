using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for person rotations
    /// </summary>
    public class StudentAvailabilityDayRepository : Repository<IStudentAvailabilityDay>, IStudentAvailabilityDayRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRotationRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public StudentAvailabilityDayRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public StudentAvailabilityDayRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
            : base(unitOfWorkFactory)
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
                .SetFetchMode("RestrictionCollection", FetchMode.Join)
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
                .Add(Restrictions.Eq("Person", person))
				.AddOrder(Order.Desc("UpdatedOn"))
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .SetFetchMode("Restriction", FetchMode.Join);
            IList<IStudentAvailabilityDay> retList = crit.List<IStudentAvailabilityDay>();

            InitializeStudentDays(retList);
            return retList;
        }

		public IList<IStudentAvailabilityDay> FindNewerThan(DateTime newerThan)
		{
			var crit = Session.CreateCriteria(typeof(StudentAvailabilityDay))
				.Add(Restrictions.Gt("UpdatedOn", newerThan))
				.SetFetchMode("RestrictionCollection", FetchMode.Join);
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

        private static ICriterion personCriterion(IList<IPerson> personCollection)
        {
            ICriterion ret;
            if (personCollection.Count > 1)
                ret = Restrictions.InG("Person", personCollection);
            else
                ret = Restrictions.Eq("Person", personCollection[0]);
            return ret;
        }

        public IStudentAvailabilityDay LoadAggregate(Guid id)
        {
            IStudentAvailabilityDay ret = Session.CreateCriteria(typeof(StudentAvailabilityDay))
                .SetFetchMode("RestrictionCollection", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .Add(Restrictions.Eq("Id", id))
                .UniqueResult<IStudentAvailabilityDay>();

            return ret;
        }
    }
}