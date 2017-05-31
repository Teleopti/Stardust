using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAssignmentRepository : Repository<IPersonAssignment>, IPersonAssignmentRepository, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>
	{
		public PersonAssignmentRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PersonAssignmentRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public virtual ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			InParameter.NotNull(nameof(persons), persons);
			var retList = new List<IPersonAssignment>();

			foreach (var personList in persons.Batch(400))
			{
				var personArray = personList.ToArray();
				var crit = personAssignmentCriteriaLoader(period, scenario);
				crit.Add(Restrictions.InG("ass.Person", personArray));
				retList.AddRange(crit.List<IPersonAssignment>());
			}

			return retList;
		}
		
		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			InParameter.NotNull(nameof(scenario), scenario);
			var crit = personAssignmentCriteriaLoader(period, scenario);
			using (PerformanceOutput.ForOperation("Loading personassignments"))
			{
				return crit.List<IPersonAssignment>();
			}
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person)
		{
			return Session.GetNamedQuery("fetchIdAndVersionPersonAssignment")
			              .SetEntity("scenario", scenario)
						  .SetDateOnly("start", period.StartDate)
						  .SetDateOnly("end", period.EndDate)
										.SetEntity("person", person)
										.SetResultTransformer(
											Transformers.AliasToBeanConstructor(typeof(DateScenarioPersonId).GetConstructor(
											new[]
												{
													typeof(Guid),
													typeof(DateTime), 
													typeof(Guid), 
													typeof(Guid),
													typeof(int)
												})))
			              .List<DateScenarioPersonId>();
		}

		private ICriteria personAssignmentCriteriaLoader(DateOnlyPeriod period, IScenario scenario)
		{
			var assCriteria = Session.CreateCriteria(typeof(PersonAssignment), "ass")
				.SetTimeout(300)
				.SetFetchMode("ShiftLayers", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity);
			addScenarioAndFilterClauses(assCriteria, scenario, period);
			return assCriteria;
		}
		
		private static void addScenarioAndFilterClauses(ICriteria criteria, IScenario scenario, DateOnlyPeriod period)
		{
			criteria.Add(Restrictions.Eq("ass.Scenario", scenario))
			        .Add(Restrictions.Between("Date", period.StartDate, period.EndDate));
		}

		public IPersonAssignment LoadAggregate(Guid id)
		{
			var ass = Session.CreateCriteria(typeof (PersonAssignment))
			                               .SetFetchMode("ShiftLayers", FetchMode.Join)
			                               .Add(Restrictions.Eq("Id", id))
			                               .UniqueResult<IPersonAssignment>();
			if (ass != null)
			{
				var initializer = new InitializeRootsPersonAssignment(new List<IPersonAssignment> {ass});
				initializer.Initialize();
			}

			return ass;
		}

		public IPersonAssignment LoadAggregate(PersonAssignmentKey id)
		{
			return Session.CreateCriteria(typeof(PersonAssignment))
						       .SetFetchMode("ShiftLayers", FetchMode.Join)
						       .Add(Restrictions.Eq("Scenario", id.Scenario))
						       .Add(Restrictions.Eq("Person", id.Person))
						       .Add(Restrictions.Eq("Date", id.Date))
						       .UniqueResult<IPersonAssignment>();
		}

		public virtual DateTime GetScheduleLoadedTime()
		{
			return Session.CreateSQLQuery("SELECT GETDATE()").UniqueResult<DateTime>();
		}

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, string source)
		{
			InParameter.NotNull(nameof(persons), persons);
			var retList = new List<IPersonAssignment>();

			foreach (var personList in persons.Batch(400))
			{
				var personArray = personList.ToArray();
				var crit = personAssignmentCriteriaLoader(period, scenario);
				crit.Add(Restrictions.InG("ass.Person", personArray));
				retList.AddRange(crit.List<IPersonAssignment>());
				crit.Add(Restrictions.Eq(nameof(IPersonAssignment.Source), source));
			}

			return retList;
		}
	}
}
