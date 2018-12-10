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
	public class PersonAvailabilityRepository : Repository<IPersonAvailability>, IPersonAvailabilityRepository
	{
		public PersonAvailabilityRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PersonAvailabilityRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<IPersonAvailability> LoadPersonAvailabilityWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			var rep2 = new AvailabilityRepository(UnitOfWork);
			rep2.LoadAvailabilitiesWithHierarchyData(persons,startDate);

			return loadForPersons(persons);
		}

		private IEnumerable<IPersonAvailability> loadForPersons(IEnumerable<IPerson> persons)
		{
		    var personAvailabilites = new List<IPersonAvailability>();
		    foreach (var personBatch in persons.Batch(400))
		    {
                personAvailabilites.AddRange(Session.CreateCriteria(typeof(PersonAvailability))
                    .Fetch("Availability")
                    .Add(Restrictions.InG("Person", personBatch.ToArray()))
                    .List<IPersonAvailability>());
		    }
		    return personAvailabilites;
		}

		public ICollection<IPersonAvailability> Find(IEnumerable<IPerson> persons,
												   DateOnlyPeriod period)
		{
			InParameter.NotNull(nameof(persons), persons);

			var personAvailabilites = new List<IPersonAvailability>();
			foreach (var personBatch in persons.Batch(400))
			{
				personAvailabilites.AddRange(Session.CreateCriteria(typeof(PersonAvailability))
					.Add(Restrictions.Between("StartDate", period.StartDate, period.EndDate))
					.Add(Restrictions.InG("Person", personBatch.ToArray()))
					.List<IPersonAvailability>());
			}
			return personAvailabilites;
		}
	}
}
