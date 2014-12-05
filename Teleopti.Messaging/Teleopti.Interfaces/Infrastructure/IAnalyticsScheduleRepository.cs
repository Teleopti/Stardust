using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleRow(IAnalyticsFactScheduleTime analyticsFactScheduleTime, AnalyticsFactScheduleDate analyticsFactScheduleDate, AnalyticsFactSchedulePerson personPart);
		void PersistFactScheduleDayCountRow(AnalyticsFactScheduleDayCount dayCount);
		void DeleteFactSchedule(DateOnly date);
		IList<KeyValuePair<DateOnly, int>> LoadDimDates(DateTime today);

		IList<IAnalyticsActivity> Activities();
		IList<IAnalyticsAbsence> Absences();

	}

	public interface IAnalyticsFactScheduleTime
	{
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

	public class AnalyticsFactSchedulePerson
	{
	}

	public class AnalyticsFactScheduleDate
	{
		public int ScheduleDateId { get; set; }
		public int ScheduleStartDateLocalId { get; set; }
		public DateTime ActivityStartTime { get; set; }
		public int ActivityStartDateId { get; set; }
		public DateTime ActivityEndTime { get; set; }
		public int ActivityEndDateId { get; set; }
		public DateTime ShiftStartTime { get; set; }
		public int ShiftStartDateId { get; set; }
		public DateTime ShiftEndTime { get; set; }
		public int ShiftEndDateId { get; set; }
		public int IntervalId { get; set; }
		public int ShiftStartIntervalId { get; set; }
		public int ShiftEndIntervalId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}


	public class AnalyticsFactScheduleDayCount
	{
	}
}