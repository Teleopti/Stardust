using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class RuleSetBagRepository : Repository<IRuleSetBag>, IRuleSetBagRepository
	{
#pragma warning disable 618
		public RuleSetBagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public RuleSetBagRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
		{
			return Session.CreateCriteria(typeof (IRuleSetBag), "bag")
				.Fetch("RuleSetCollection")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IRuleSetBag>();
		}

		/// <remarks>Don't remove the last 2 criterias, by doing it this way it will load the referred objects directly 
		/// instead of ending up with lazy load calls later on. That has a significant difference when you’re loading 
		/// shiftbags with many rulesets.</remarks>
		public IRuleSetBag FindWithRuleSetsAndAccessibility(Guid id)
		{
			var mainCrit = DetachedCriteria.For<RuleSetBag>()
				.Add(Restrictions.Eq("Id", id))
				.Fetch("RuleSetCollection");

			var subCrit = DetachedCriteria.For<RuleSetBag>("b")
				.Add(Restrictions.Eq("b.Id", id))
				.CreateAlias("RuleSetCollection", "r", JoinType.InnerJoin)
				.SetProjection(Projections.Property("r.Id"));

			var extCritTwo = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
				.Fetch("AccessibilityDates");

			var extCritThree = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
				.Fetch("AccessibilityDaysOfWeek");

			var res = Session.CreateQueryBatch()
				.Add<RuleSetBag>(mainCrit)
				.Add<WorkShiftRuleSet>(extCritTwo)
				.Add<WorkShiftRuleSet>(extCritThree);

			var ruleSetBags = CollectionHelper.ToDistinctGenericCollection<IRuleSetBag>(res.GetResult<RuleSetBag>(0));

			return ruleSetBags.FirstOrDefault();
		}

		public IRuleSetBag[] FindWithRuleSetsAndAccessibility(Guid[] ruleBagIds)
		{
			var mainCrit = DetachedCriteria.For<RuleSetBag>()
				.Add(Restrictions.In("Id", ruleBagIds))
				.Fetch("RuleSetCollection");

			var subCrit = DetachedCriteria.For<RuleSetBag>("b")
				.Add(Restrictions.In("b.Id", ruleBagIds))
				.CreateAlias("RuleSetCollection", "r", JoinType.InnerJoin)
				.SetProjection(Projections.Property("r.Id"));

			var extCritTwo = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
				.Fetch("AccessibilityDates");

			var extCritThree = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
				.Fetch("AccessibilityDaysOfWeek");

			var res = Session.CreateQueryBatch()
				.Add<RuleSetBag>(mainCrit)
				.Add<WorkShiftRuleSet>(extCritTwo)
				.Add<WorkShiftRuleSet>(extCritThree);

			var ruleSetBags = CollectionHelper.ToDistinctGenericCollection<IRuleSetBag>(res.GetResult<RuleSetBag>(0));

			return ruleSetBags.ToArray();
		}
	}
}
