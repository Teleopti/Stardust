using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for person rotations
    /// </summary>
    public class PersonRotationRepository : Repository<IPersonRotation>, IPersonRotationRepository
    {
        public PersonRotationRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
		}

		public PersonRotationRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public IEnumerable<IPersonRotation> LoadPersonRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			var rep2 = new RotationRepository(UnitOfWork);
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
