using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Multi;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for availability types
    /// </summary>
    public class AvailabilityRepository : Repository<IAvailabilityRotation>, ILoadAggregateById<IAvailabilityRotation>
    {
#pragma warning disable 618
        public AvailabilityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

        /// <summary>
        /// Loads all availabilities with hierarchy data.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-20    
        /// </remarks>
        public IList<IAvailabilityRotation> LoadAllAvailabilitiesWithHierarchyData()
        {
            var availabilityRotation = DetachedCriteria.For<AvailabilityRotation>("availabilityRotation")
                .Fetch("AvailabilityDays");

            var availabilityDay = DetachedCriteria.For<AvailabilityDay>("availabilityDay")
                .Fetch("Restriction");

            return CollectionHelper.ToDistinctGenericCollection<IAvailabilityRotation>(Session.CreateQueryBatch()
                .Add<AvailabilityRotation>(availabilityRotation)
                .Add<AvailabilityDay>(availabilityDay)
                .GetResult<AvailabilityRotation>(0)).ToList();
        }

        public IAvailabilityRotation LoadAggregate(Guid id)
        {
            IAvailabilityRotation ret = Session.CreateCriteria(typeof(AvailabilityRotation))
                        .Fetch("AvailabilityDays")
                        .Fetch("AvailabilityDays.Restriction")
                        .SetResultTransformer(Transformers.DistinctRootEntity)
                        .Add(Restrictions.Eq("Id", id))
                .UniqueResult<IAvailabilityRotation>();

            return ret;
        }

		public IEnumerable<IAvailabilityRotation> LoadAvailabilitiesWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
    	{
			var retList = new HashSet<IAvailabilityRotation>();
			foreach (var personsPart in persons.Batch(400))
			{
				var tempResult = Session.GetNamedQuery("LoadAvailabilityRotationsWithHierarchyData")
						.SetDateOnly("StartDate", startDate)
						.SetParameterList("PersonCollection", personsPart)
						.List<IAvailabilityRotation>();
				foreach (var rotation in tempResult)
				{
					retList.Add(rotation);
				}
			}
			return retList;
    	}

        public IEnumerable<IAvailabilityRotation> LoadAllSortedByNameAscending()
        {
            return Session.Query<IAvailabilityRotation>().OrderBy(ar => ar.Name).ToList();
        }
    }
}
