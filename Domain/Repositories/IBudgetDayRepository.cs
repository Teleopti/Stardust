using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IBudgetDayRepository : IRepository<IBudgetDay>
    {
        DateOnly FindLastDayWithStaffEmployed(IScenario scenario, IBudgetGroup budgetGroup, DateOnly lastDateToSearch);
        IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod);
		[Obsolete("Don't use! Shouldn't be here - use ICurrentUnitOfWork instead (or get the unitofwork in some other way).")]
		IUnitOfWork UnitOfWork { get; }
	}
}