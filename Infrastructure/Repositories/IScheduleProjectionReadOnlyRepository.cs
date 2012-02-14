using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IScheduleProjectionReadOnlyRepository
	{
		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period,IBudgetGroup budgetGroup,IScenario scenario);
		void ClearPeriodForPerson(DateOnlyPeriod period, IScenario scenario, Guid personId);
		void AddProjectedLayer(DateOnly belongsToDate,IScenario scenario,Guid personId, IVisualLayer visualLayer);
		bool IsInitialized();
	}
}