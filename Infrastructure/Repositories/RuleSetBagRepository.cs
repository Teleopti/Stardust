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
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class RuleSetBagRepository : Repository<IRuleSetBag>, IRuleSetBagRepository
	{
		public static RuleSetBagRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new RuleSetBagRepository(currentUnitOfWork, null, null);
		}

		public static RuleSetBagRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new RuleSetBagRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}
		
		public RuleSetBagRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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
