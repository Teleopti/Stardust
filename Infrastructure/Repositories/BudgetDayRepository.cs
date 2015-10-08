using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BudgetDayRepository : Repository<IBudgetDay>, IBudgetDayRepository
    {
#pragma warning disable 618
        public BudgetDayRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
#pragma warning restore 618
        {
        }

        public BudgetDayRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

	    public BudgetDayRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
	    {
		  }


        public DateOnly FindLastDayWithStaffEmployed(IScenario scenario, IBudgetGroup budgetGroup, DateOnly lastDateToSearch)
        {
            var foundBudgetDays = Session.CreateCriteria(typeof(BudgetDay))
                .Add(Restrictions.Eq("Scenario", scenario))
                .Add(Restrictions.Eq("BudgetGroup", budgetGroup))
                .Add(Restrictions.Le("Day", lastDateToSearch))
                .Add(Restrictions.IsNotNull("StaffEmployed"))
                .AddOrder(Order.Desc("Day"))
                .List<IBudgetDay>();
            var lastBudgetDay = foundBudgetDays.FirstOrDefault();
            if (lastBudgetDay == null) return lastDateToSearch;
            return lastBudgetDay.Day;
        }

        public IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod)
        {
            var foundBudgetDaysSubquery = DetachedCriteria.For<BudgetDay>()
                .Add(Restrictions.Eq("Scenario", scenario))
                .Add(Restrictions.Eq("BudgetGroup", budgetGroup))
                .Add(Restrictions.Between("Day", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
                .SetProjection(Projections.Property("Id"));

            var customShrinkages = DetachedCriteria.For<BudgetDay>()
               .Add(Subqueries.PropertyIn("Id", foundBudgetDaysSubquery))
               .SetFetchMode("CustomShrinkages", FetchMode.Join);

            var customEfficiencyShrinkages = DetachedCriteria.For<BudgetDay>()
                .Add(Subqueries.PropertyIn("Id", foundBudgetDaysSubquery))
                .SetFetchMode("CustomEfficiencyShrinkages", FetchMode.Join);

            var result = Session.CreateMultiCriteria().Add(customShrinkages).Add(customEfficiencyShrinkages).List();
            var foundBudgetDays = CollectionHelper.ToDistinctGenericCollection<IBudgetDay>(result[0]).ToList();
            return foundBudgetDays;
        }
    }
}
