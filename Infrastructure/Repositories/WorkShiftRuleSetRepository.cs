using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for WorkShiftRuleSet
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-27
    /// </remarks>
    public class WorkShiftRuleSetRepository : Repository<IWorkShiftRuleSet>, IWorkShiftRuleSetRepository
    {
#pragma warning disable 618
        public WorkShiftRuleSetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

		public WorkShiftRuleSetRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all with limiters and extenders included.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-15
        /// </remarks>
        public ICollection<IWorkShiftRuleSet> FindAllWithLimitersAndExtenders()
        {
            DetachedCriteria mainCrit = DetachedCriteria.For<WorkShiftRuleSet>()
                                .Fetch("LimiterCollection");

            DetachedCriteria extCrit = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .Fetch("ExtenderCollection");

            DetachedCriteria extCritTwo = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .Fetch("AccessibilityDates");

            DetachedCriteria extCritThree = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .Fetch("AccessibilityDaysOfWeek");

            DetachedCriteria extCritFour = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .Fetch("RuleSetBagCollection");

            var res = Session.CreateQueryBatch()
                                .Add<WorkShiftRuleSet>(mainCrit)
                                .Add<WorkShiftRuleSet>(extCrit)
                                .Add<WorkShiftRuleSet>(extCritTwo)
                                .Add<WorkShiftRuleSet>(extCritThree)
                                .Add<WorkShiftRuleSet>(extCritFour)
                                .GetResult<WorkShiftRuleSet>(0);

            ICollection<IWorkShiftRuleSet> ruleSets =
                CollectionHelper.ToDistinctGenericCollection<IWorkShiftRuleSet>(res);

            initializeRoots(ruleSets);

            return ruleSets;
        }

        private static void initializeRoots(IEnumerable<IWorkShiftRuleSet> ruleSets)
        {
            foreach (IWorkShiftRuleSet ruleSet in ruleSets)
            {
                LazyLoadingManager.Initialize(ruleSet.TemplateGenerator.Category); //shift category
                LazyLoadingManager.Initialize(ruleSet.TemplateGenerator.BaseActivity); //activity
                foreach (IWorkShiftExtender extender in ruleSet.ExtenderCollection)
                {
                    LazyLoadingManager.Initialize(extender.ExtendWithActivity); //activity
                }
            }
        }
    }
}
