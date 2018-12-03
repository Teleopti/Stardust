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
	public class OvertimeAvailabilityRepository : Repository<IOvertimeAvailability>, IOvertimeAvailabilityRepository
	{
		public OvertimeAvailabilityRepository(ICurrentUnitOfWork currentUnitOfWork)
			 : base(currentUnitOfWork)
		{

		}

		public IList<IOvertimeAvailability> Find(DateOnly dateOnly, IPerson person)
		{
			ICriteria crit = Session.CreateCriteria(typeof(OvertimeAvailability))
											.Add(Restrictions.Eq("DateOfOvertime", dateOnly))
											.Add(personCriterion(person))
											.SetResultTransformer(Transformers.DistinctRootEntity)
											.SetFetchMode("Restriction", FetchMode.Join);
			IList<IOvertimeAvailability> retList = crit.List<IOvertimeAvailability>();

			initializeStudentDays(retList);
			return retList;
		}

		public IList<IOvertimeAvailability> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons)
		{
			var result = new List<IOvertimeAvailability>();

			if (!persons.Any()) return result;

			foreach (var personList in persons.Batch(400))
			{
				ICriteria crit = filterByPeriod(period)
					 .Add(personCriterion(personList.ToArray()))
					 .SetResultTransformer(Transformers.DistinctRootEntity);
				result.AddRange(crit.List<IOvertimeAvailability>());
			}

			initializeStudentDays(result);
			return result;
		}

		public IList<IOvertimeAvailability> Find(DateOnlyPeriod period)
		{
			var result = new List<IOvertimeAvailability>();

			ICriteria crit = filterByPeriod(period).SetResultTransformer(Transformers.DistinctRootEntity);
			result.AddRange(crit.List<IOvertimeAvailability>());

			initializeStudentDays(result);
			return result;
		}

		private ICriteria filterByPeriod(DateOnlyPeriod period)
		{
			return Session.CreateCriteria(typeof(OvertimeAvailability))

							  .Add(Restrictions.Between("DateOfOvertime", period.StartDate, period.EndDate))
							  .AddOrder(Order.Asc("Person"))
							  .AddOrder(Order.Asc("DateOfOvertime"));
		}

		private static void initializeStudentDays(IEnumerable<IOvertimeAvailability> overtimeDays)
		{
			foreach (IOvertimeAvailability day in overtimeDays)
			{
				if (!LazyLoadingManager.IsInitialized(day.Person))
					LazyLoadingManager.Initialize(day.Person);
			}
		}

		private static ICriterion personCriterion(params IPerson[] people)
		{
			ICriterion ret;
			if (people.Length > 1)
				ret = Restrictions.InG("Person", people);
			else
				ret = Restrictions.Eq("Person", people[0]);
			return ret;
		}

		public IOvertimeAvailability LoadAggregate(Guid id)
		{
			IOvertimeAvailability ret = Session.CreateCriteria(typeof(OvertimeAvailability))
															 .Add(Restrictions.Eq("Id", id))
															 .UniqueResult<IOvertimeAvailability>();

			return ret;
		}


	}
}