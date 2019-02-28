using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_QueryHintOnLayers_79780)]
	public class PersonAssignmentRepositoryWithQueryHint : PersonAssignmentRepository
	{
		public PersonAssignmentRepositoryWithQueryHint(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) : 
			base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public override IEnumerable<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			using (SqlModificationScope.Create(new AddForceSeekOnAssignmentLayers()))
			{
				return base.Find(persons, period, scenario);				
			}
		}
	}
	
	public class PersonAssignmentRepository : Repository<IPersonAssignment>, IPersonAssignmentRepository,
		IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>
	{
		public static PersonAssignmentRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonAssignmentRepository(currentUnitOfWork, null, null);
		}

		public PersonAssignmentRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		[RemoveMeWithToggle("make non virtual", Toggles.ResourcePlanner_QueryHintOnLayers_79780)]
		public virtual IEnumerable<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
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
				.Fetch("ShiftLayers")
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
			                               .Fetch("ShiftLayers")
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
						       .Fetch("ShiftLayers")
						       .Add(Restrictions.Eq("Scenario", id.Scenario))
						       .Add(Restrictions.Eq("Person", id.Person))
						       .Add(Restrictions.Eq("Date", id.Date))
						       .UniqueResult<IPersonAssignment>();
		}

		public virtual DateTime GetScheduleLoadedTime()
		{
			return Session.CreateSQLQuery("SELECT GETDATE()").UniqueResult<DateTime>();
		}

		public IEnumerable<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, string source)
		{
			InParameter.NotNull(nameof(persons), persons);
			var retList = new List<IPersonAssignment>();

			foreach (var personList in persons.Batch(400))
			{
				var personArray = personList.ToArray();
				var crit = personAssignmentCriteriaLoader(period, scenario);
				crit.Add(Restrictions.InG("ass.Person", personArray));
				crit.Add(Restrictions.Eq(nameof(IPersonAssignment.Source), source));
				retList.AddRange(crit.List<IPersonAssignment>());
			}

			return retList;
		}

		public bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period)
		{
			var sql = $@"IF EXISTS (SELECT TOP 1 pa.Id
  FROM PersonAssignment pa
 INNER JOIN Person p ON pa.Person = p.Id
 INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
 INNER JOIN Team t ON pp.Team = t.Id
 INNER JOIN [Site] s ON t.[Site] = s.Id
 INNER JOIN BusinessUnit bu ON s.BusinessUnit = bu.Id
 WHERE bu.Id = :{nameof(businessUnitId)}
   AND pa.Date >= :startDate
   AND pa.Date <= :endDate)
SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)";
			var result = Session.CreateSQLQuery(sql)
				.SetParameter(nameof(businessUnitId), businessUnitId)
				.SetDateTime("startDate", period.StartDate.Date)
				.SetDateTime("endDate", period.EndDate.Date)
				.UniqueResult<bool>();
			return result;
		}
	}
}
