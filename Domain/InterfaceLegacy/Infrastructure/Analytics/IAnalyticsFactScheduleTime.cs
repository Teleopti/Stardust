namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics
{
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
		int ReadyTimeMinutes { get; set; }  
		int PaidTimeMinutes { get; set; }  
		int PaidTimeActivityMinutes { get; set; }
		int PaidTimeAbsenceMinutes { get; set; }
		int ShiftLengthId { get; set; }
		int PlannedOvertimeMinutes { get; set; }
	}
}