using System.Collections;
using System.Collections.Generic;
using NHibernate;
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
                                .SetFetchMode("LimiterCollection", FetchMode.Join);

            DetachedCriteria extCrit = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .SetFetchMode("ExtenderCollection", FetchMode.Join);

            DetachedCriteria extCritTwo = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .SetFetchMode("AccessibilityDates", FetchMode.Join);

            DetachedCriteria extCritThree = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .SetFetchMode("AccessibilityDaysOfWeek", FetchMode.Join);

            DetachedCriteria extCritFour = DetachedCriteria.For<WorkShiftRuleSet>()
                                                        .SetFetchMode("RuleSetBagCollection", FetchMode.Join);

            IList res = Session.CreateMultiCriteria()
                                .Add(mainCrit)
                                .Add(extCrit)
                                .Add(extCritTwo)
                                .Add(extCritThree)
                                .Add(extCritFour)
                                .List();

            ICollection<IWorkShiftRuleSet> ruleSets =
                CollectionHelper.ToDistinctGenericCollection<IWorkShiftRuleSet>(res[0]);

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
