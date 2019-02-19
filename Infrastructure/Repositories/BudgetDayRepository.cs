using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BudgetDayRepository : Repository<IBudgetDay>, IBudgetDayRepository
    {
	    public BudgetDayRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
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

        public IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod, bool noLock = false)
        {
			
			var foundBudgetDaysSubquery = DetachedCriteria.For<BudgetDay>()
                .Add(Restrictions.Eq("Scenario", scenario))
                .Add(Restrictions.Eq("BudgetGroup", budgetGroup))
                .Add(Restrictions.Between("Day", dateOnlyPeriod.StartDate, dateOnlyPeriod.EndDate))
                .SetProjection(Projections.Property("Id"));

            var customShrinkages = DetachedCriteria.For<BudgetDay>()
               .Add(Subqueries.PropertyIn("Id", foundBudgetDaysSubquery))
               .Fetch("CustomShrinkages");

            var customEfficiencyShrinkages = DetachedCriteria.For<BudgetDay>()
                .Add(Subqueries.PropertyIn("Id", foundBudgetDaysSubquery))
                .Fetch("CustomEfficiencyShrinkages");
	        IList<BudgetDay> result;
	        if (noLock)
	        {
				using (Session.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					result = Session.CreateQueryBatch().Add<BudgetDay>(customShrinkages).Add<BudgetDay>(customEfficiencyShrinkages).GetResult<BudgetDay>(0);
				}
			}else
				result = Session.CreateQueryBatch().Add<BudgetDay>(customShrinkages).Add<BudgetDay>(customEfficiencyShrinkages).GetResult<BudgetDay>(0);


			var foundBudgetDays = CollectionHelper.ToDistinctGenericCollection<IBudgetDay>(result).ToList();
            return foundBudgetDays;
        }
    }
}
