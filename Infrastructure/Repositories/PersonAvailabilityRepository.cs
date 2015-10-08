using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public IEnumerable<IPersonAvailability> LoadPersonAvailabilityWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			var rep2 = new AvailabilityRepository(UnitOfWork);
			rep2.LoadAvailabilitiesWithHierarchyData(persons,startDate);

			return LoadForPersons(persons);
		}

		private IEnumerable<IPersonAvailability> LoadForPersons(IEnumerable<IPerson> persons)
		{
		    var personAvailabilites = new List<IPersonAvailability>();
		    foreach (var personBatch in persons.Batch(400))
		    {
                personAvailabilites.AddRange(Session.CreateCriteria(typeof(PersonAvailability))
                    .SetFetchMode("Availability",FetchMode.Join)
                    .Add(Restrictions.In("Person", personBatch.ToArray()))
                    .List<IPersonAvailability>());
		    }
		    return personAvailabilites;
		}

		public ICollection<IPersonAvailability> Find(IEnumerable<IPerson> persons,
												   DateOnlyPeriod period)
		{
			InParameter.NotNull("persons", persons);

			var personAvailabilites = new List<IPersonAvailability>();
			foreach (var personBatch in persons.Batch(400))
			{
				personAvailabilites.AddRange(Session.CreateCriteria(typeof(PersonAvailability))
					.Add(Restrictions.Between("StartDate", period.StartDate, period.EndDate))
					.Add(Restrictions.In("Person", personBatch.ToArray()))
					.List<IPersonAvailability>());
			}
			return personAvailabilites;
		}
	}
}
