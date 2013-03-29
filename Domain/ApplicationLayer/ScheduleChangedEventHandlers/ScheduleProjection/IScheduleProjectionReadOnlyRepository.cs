using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleProjectionReadOnlyRepository
	{
		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period,IBudgetGroup budgetGroup,IScenario scenario);
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid scenarioId, Guid personId);
		void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, ProjectionChangedEventLayer layer);
		bool IsInitialized();
	}
}