﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for RuleSetBag
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-28
    /// </remarks>
    public class RuleSetBagRepository : Repository<IRuleSetBag>, IRuleSetBagRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSetBagRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        public RuleSetBagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

	    public RuleSetBagRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
		    
	    }

        public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
        {
            return Session.CreateCriteria(typeof (IRuleSetBag), "bag")
                .SetFetchMode("RuleSetCollection", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IRuleSetBag>();
        }

	    public IRuleSetBag Find(Guid id)
	    {
			var mainCrit = DetachedCriteria.For<RuleSetBag>()
					.Add(Restrictions.Eq("Id", id))
					.SetFetchMode("RuleSetCollection", FetchMode.Join);

			var subCrit = DetachedCriteria.For<RuleSetBag>("b")
					.Add(Restrictions.Eq("b.Id", id))
					.CreateAlias("RuleSetCollection", "r", JoinType.InnerJoin)
					.SetProjection(Projections.Property("r.Id"));

			var extCrit = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
					.SetFetchMode("ExtenderCollection", FetchMode.Join);

			var extCritTwo = DetachedCriteria.For<WorkShiftRuleSet>()
					.Add(Subqueries.PropertyIn("Id", subCrit))
					.SetFetchMode("AccessibilityDates", FetchMode.Join);

			var extCritThree = DetachedCriteria.For<WorkShiftRuleSet>()
				.Add(Subqueries.PropertyIn("Id", subCrit))
			    .SetFetchMode("AccessibilityDaysOfWeek", FetchMode.Join);

			var res = Session.CreateMultiCriteria()
								.Add(mainCrit)
								.Add(extCrit)
								.Add(extCritTwo)
								.Add(extCritThree)
								.List();

			var ruleSetBags =
				CollectionHelper.ToDistinctGenericCollection<IRuleSetBag>(res[0]);

		    return ruleSetBags.FirstOrDefault();
			
			
			
			/* Session.CreateCriteria(typeof(IRuleSetBag), "bag")
				.Add(Restrictions.Eq("Id",id))
				.CreateAlias("RuleSetCollection","r",JoinType.InnerJoin)
				.CreateAlias("r.LimiterCollection", "l", JoinType.LeftOuterJoin)
				.CreateAlias("r.ExtenderCollection", "e", JoinType.LeftOuterJoin)
				.CreateAlias("r.AccessibilityDates", "d", JoinType.LeftOuterJoin)
				.CreateAlias("r.AccessibilityDaysOfWeek", "w", JoinType.LeftOuterJoin)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.UniqueResult<IRuleSetBag>();*/
	    }
    }
}
