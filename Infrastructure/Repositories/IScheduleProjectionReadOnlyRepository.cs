using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IScheduleProjectionReadOnlyRepository
	{
		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period,IBudgetGroup budgetGroup,IScenario scenario);
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid scenarioId, Guid personId);
		void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, DenormalizedScheduleProjectionLayer layer);
		bool IsInitialized();
	    DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId);
	}
}