using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyModel
	{
		public Guid PersonId { get; set; }
		public Guid ScenarioId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public Guid PayloadId { get; set; }
		public TimeSpan WorkTime { get; set; }
		public TimeSpan ContractTime { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public int DisplayColor { get; set; }
		public DateTime ScheduleLoadedTime { get; set; }
	}
	
	public interface IScheduleProjectionReadOnlyPersister
	{
		bool IsInitialized();

		int AddProjectedLayer(ScheduleProjectionReadOnlyModel model);
		int ClearDayForPerson(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp);

		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario);
		int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate);
		
		IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId);

		DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId);
	}
}