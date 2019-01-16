using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public interface IScheduleProjectionReadOnlyPersister
	{
		bool IsInitialized();

		bool BeginAddingSchedule(DateOnly date, Guid scenarioId, Guid personId, int version);
		void AddActivity(IEnumerable<ScheduleProjectionReadOnlyModel> models);

		IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario);
		int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate);

		IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId);
	}

	public class ScheduleProjectionReadOnlyModel : IEquatable<ScheduleProjectionReadOnlyModel>
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
		public bool Equals(ScheduleProjectionReadOnlyModel other)
		{
			if (other == null) return false; 
			return (PersonId == other.PersonId && ScenarioId == other.ScenarioId && BelongsToDate == other.BelongsToDate
				&& PayloadId == other.PayloadId && WorkTime == other.WorkTime
				&& ContractTime == other.ContractTime
				&& StartDateTime == other.StartDateTime
				&& EndDateTime == other.EndDateTime
				&& Name == other.Name
				&& ShortName == other.ShortName
				&& DisplayColor == other.DisplayColor);
		}
	}

}