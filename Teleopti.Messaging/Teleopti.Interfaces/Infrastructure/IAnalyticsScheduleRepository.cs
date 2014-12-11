using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleRow(IAnalyticsFactScheduleTime timePart, IAnalyticsFactScheduleDate datePart, IAnalyticsFactSchedulePerson personPart);
		void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount);
		void DeleteFactSchedule(int date, int personId);
		IList<KeyValuePair<DateOnly, int>> LoadDimDates();

		IList<IAnalyticsActivity> Activities();
		IList<IAnalyticsAbsence> Absences();
		IList<IAnalyticsGeneric> Scenarios();
		IList<IAnalyticsGeneric> ShiftCategories();
		IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode);
	}

	public interface IAnalyticsFactScheduleDayCount
	{
		int ShiftStartDateLocalId { get; set; }
		int PersonId { get; set; }
		int BusinessUnitId { get; set; }
		int ScenarioId { get; set; }
		int ShiftCategoryId { get; set; }
		string DayOffName { get; set; }
		string DayOffShortName { get; set; }
		int AbsenceId { get; set; }
		DateTime StartTime { get; set; }
	}

	public interface IAnalyticsFactScheduleTime
	{
		int ShiftCategoryId { get; set; } 
		int ScenarioId { get; set; }
		int ActivityId { get; set; } 
		int AbsenceId  { get; set; } 
		int OverTimeId  { get; set; } 
		int ScheduledMinutes { get; set; }  
		int ScheduledAbsenceMinutes { get; set; }  
		int ScheduledActivityMinutes { get; set; }  
		int ContractTimeMinutes { get; set; }  
		int ContractTimeActivityMinutes { get; set; }  
		int ContractTimeAbsenceMinutes { get; set; }  
		int WorkTimeMinutes { get; set; }  
		int WorkTimeActivityMinutes { get; set; } 
		int WorkTimeAbsenceMinutes	{ get; set; } 
		int OverTimeMinutes { get; set; }  
		int ReadyTimeMinues { get; set; }  
		int PaidTimeMinutes { get; set; }  
		int PaidTimeActivityMinutes { get; set; }
		int PaidTimeAbsenceMinutes { get; set; }
		int ShiftLength { get; set; }
	}

	public interface IAnalyticsFactScheduleDate
	{
		int ScheduleDateId { get; set; }
		int ScheduleStartDateLocalId { get; set; }
		DateTime ActivityStartTime { get; set; }
		int ActivityStartDateId { get; set; }
		DateTime ActivityEndTime { get; set; }
		int ActivityEndDateId { get; set; }
		DateTime ShiftStartTime { get; set; }
		int ShiftStartDateId { get; set; }
		DateTime ShiftEndTime { get; set; }
		int ShiftEndDateId { get; set; }
		int IntervalId { get; set; }
		int ShiftStartIntervalId { get; set; }
		int ShiftEndIntervalId { get; set; }
		DateTime DatasourceUpdateDate { get; set; }
	}

	public interface IAnalyticsActivity
	{
		int ActivityId { get; set; }
		Guid ActivityCode { get; set; } 
		bool InPaidTime { get; set; }
		bool InReadyTime { get; set; } 
	}

	public interface IAnalyticsAbsence
	{
		int AbsenceId { get; set; }
		Guid AbsenceCode { get; set; }
		bool InPaidTime { get; set; }
	}

	public interface IAnalyticsPersonBusinessUnit
	{
		int PersonId { get; set; }
		int BusinessUnitId { get; set; }
	}

	public interface IAnalyticsGeneric
	{
		int Id { get; set; }
		Guid Code { get; set; }
	}

	public interface IAnalyticsFactSchedulePerson
	{
		int PersonId { get; set; }
		int BusinessUnitId { get; set; }
	}
}