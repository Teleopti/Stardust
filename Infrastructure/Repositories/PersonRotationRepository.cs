using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for person rotations
    /// </summary>
    public class PersonRotationRepository : Repository<IPersonRotation>, IPersonRotationRepository
    {
		public static PersonRotationRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new PersonRotationRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public static PersonRotationRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonRotationRepository(currentUnitOfWork, null, null);
		}

		public PersonRotationRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IEnumerable<IPersonRotation> LoadPersonRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			var rep2 = RotationRepository.DONT_USE_CTOR(UnitOfWork);
			rep2.LoadRotationsWithHierarchyData(persons,startDate);

			return LoadForPersons(persons);
		}

    	private IEnumerable<IPersonRotation> LoadForPersons(IEnumerable<IPerson> persons)
    	{
            var personRotations = new List<IPersonRotation>();
            foreach (var personBatch in persons.Batch(400))
            {
                personRotations.AddRange(Session.CreateCriteria(typeof (PersonRotation))
                                             .Fetch("Rotation")
                                             .Add(Restrictions.InG("Person", personBatch.ToArray()))
                                             .List<IPersonRotation>());
            }
    	    return personRotations;
    	}

        /// <summary>
        /// Finds the person rotations for one person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public IList<IPersonRotation> Find(IPerson person)
        {
            InParameter.NotNull(nameof(person),person);

            return Find(new List<IPerson> {person});
        }

        /// <summary>
        /// Finds the person rotations by persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public IList<IPersonRotation> Find(IList<IPerson> persons)
        {
            InParameter.NotNull(nameof(persons), persons);

			var personRotations = new List<IPersonRotation>();
			foreach (var personBatch in persons.Batch(400))
			{
				personRotations.AddRange(Session.CreateCriteria(typeof(PersonRotation))
					.Add(Restrictions.In("Person", personBatch.ToArray()))
					.AddOrder(Order.Asc("Person"))
					.AddOrder(Order.Asc("StartDate"))
					.List<IPersonRotation>());
			}
			return personRotations;
        }
    }
}
