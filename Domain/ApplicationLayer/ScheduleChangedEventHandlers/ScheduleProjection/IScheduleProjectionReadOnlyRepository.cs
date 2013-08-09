using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public interface IScheduleProjectionReadOnlyRepository
	{
		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period,IBudgetGroup budgetGroup,IScenario scenario);
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid scenarioId, Guid personId);
		void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, ProjectionChangedEventLayer layer);
	    int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate);
		bool IsInitialized();
	    DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId);
		IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnly date, Guid personId, Guid scenarioId);
	}
}