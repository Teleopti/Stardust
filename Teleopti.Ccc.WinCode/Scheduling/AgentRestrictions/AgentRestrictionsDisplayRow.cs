using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public enum AgentRestrictionDisplayRowColumn
	{
		AgentName = 0,
		Warnings = 1,
		Type = 2,
		From = 3,
		To = 4,
		ContractTargetTime = 5,
		DaysOff = 6,
		ContractTime = 7,
		DaysOffSchedule = 8,
		Min = 9,
		Max = 10,
		DaysOffScheduleRestrictions = 11,
		Ok = 12,
		None = 13
	}

	public enum AgentRestrictionDisplayRowState
	{
		NotAvailable,
		Loading,
		Available
	}

	public interface  IAgentRestrictionsDisplayRow
	{
		string AgentName { get; set; }
		AgentRestrictionDisplayRowState State { get; set; }
		int Warnings { get; }
		string PeriodType { get; }
		string StartDate { get; }
		string EndDate { get; }
		int TargetDaysOff { get; }
		string TargetDaysOffWithTolerance { get; }	
	}

	public interface IAgentDisplayData
	{
		IScheduleMatrixPro Matrix { get; }
		TimeSpan MinimumPossibleTime { get; set; }
		TimeSpan MaximumPossibleTime { get; set; }
		int ScheduledAndRestrictionDaysOff { get; set; }
		TimeSpan ContractCurrentTime { get; set; }
		TimeSpan ContractTargetTime { get; set; }
		string ContractTargetTimeWithTolerance { get; }
		string ContractTargetTimeHourlyEmployees { get; }
		int CurrentDaysOff { get; set; }
		TimePeriod MinMaxTime { get; set; }
		string Ok { get; }
		bool NoWorkshiftFound { get; set; }
	}

	public sealed class AgentRestrictionsDisplayRow : IAgentRestrictionsDisplayRow, IAgentDisplayData
	{
		private readonly IScheduleMatrixPro _matrix;
		TimeSpan IAgentDisplayData.MinimumPossibleTime { get; set; }
		TimeSpan IAgentDisplayData.MaximumPossibleTime { get; set; }
		int IAgentDisplayData.ScheduledAndRestrictionDaysOff { get; set; }
		public AgentRestrictionDisplayRowState State { get; set; }
		public string AgentName { get; set; }
		public TimeSpan ContractTargetTime { get; set; }
		public TimeSpan ContractCurrentTime { get; set; }
		public int CurrentDaysOff { get; set; }
		private readonly IList<AgentRestrictionDisplayRowColumn> _warnings; 
		public TimePeriod MinMaxTime { get; set; }
		//public int ThreadIndex { get; set; }

		public AgentRestrictionsDisplayRow(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
			State = AgentRestrictionDisplayRowState.NotAvailable;
			_warnings = new List<AgentRestrictionDisplayRowColumn>();
		}

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}

		public void SetWarnings()
		{
			_warnings.Clear();

			if (ContractCurrentTime < MinMaxTime.StartTime || ContractCurrentTime > MinMaxTime.EndTime) _warnings.Add(AgentRestrictionDisplayRowColumn.ContractTime);
			if (!CurrentDaysOff.Equals(TargetDaysOff)) _warnings.Add(AgentRestrictionDisplayRowColumn.DaysOffSchedule);
			if (((IAgentDisplayData)this).MinimumPossibleTime > MinMaxTime.EndTime) _warnings.Add(AgentRestrictionDisplayRowColumn.Min);
			if (((IAgentDisplayData)this).MaximumPossibleTime < MinMaxTime.StartTime) _warnings.Add(AgentRestrictionDisplayRowColumn.Max);
			if(NoWorkshiftFound) _warnings.Add(AgentRestrictionDisplayRowColumn.Ok);
		}

		public string Warning(int index)
		{
			if (_warnings.Count == 0) return null;
			var column = (AgentRestrictionDisplayRowColumn)index;
			if (!_warnings.Contains(column)) return null;
			
			if (column.Equals(AgentRestrictionDisplayRowColumn.ContractTime)) return UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime;
			if (column.Equals(AgentRestrictionDisplayRowColumn.DaysOffSchedule)) return UserTexts.Resources.WrongNumberOfDaysOff;
			if (column.Equals(AgentRestrictionDisplayRowColumn.Min)) return UserTexts.Resources.LowestPossibleWorkTimeIsTooHigh;
			if (column.Equals(AgentRestrictionDisplayRowColumn.Max)) return UserTexts.Resources.HighestPossibleWorkTimeIsTooLow;
			if (column.Equals(AgentRestrictionDisplayRowColumn.Ok)) return UserTexts.Resources.AtLeastOneDayCannotBeScheduled;
			
			return null;	
		}

		public int Warnings
		{
			get { return _warnings.Count; }
		}

		public string PeriodType
		{
			get { return LanguageResourceHelper.TranslateEnumValue(_matrix.SchedulePeriod.PeriodType); }
		}

		public string StartDate
		{
			get { return _matrix.SchedulePeriod.DateOnlyPeriod.StartDate.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture); }
		}

		public string EndDate
		{
			get { return _matrix.SchedulePeriod.DateOnlyPeriod.EndDate.ToShortDateString(TeleoptiPrincipal.Current.Regional.Culture); }
		}

		public int TargetDaysOff
		{
			get{return _matrix.SchedulePeriod.Contract.EmploymentType != EmploymentType.HourlyStaff ? _matrix.SchedulePeriod.DaysOff() : 0;}
		}

		public string Ok
		{
			get {return _warnings.Count > 0 ? UserTexts.Resources.No : UserTexts.Resources.Yes;}
		}

		public bool NoWorkshiftFound { get; set; }

		public string ContractTargetTimeWithTolerance
		{
			get
			{	
				return TimeHelper.GetLongHourMinuteTimeString(ContractTargetTime, TeleoptiPrincipal.Current.Regional.Culture)	 + 
					" (" + TimeHelper.GetLongHourMinuteTimeString(MinMaxTime.StartTime, TeleoptiPrincipal.Current.Regional.Culture) + 
					" - " + TimeHelper.GetLongHourMinuteTimeString(MinMaxTime.EndTime, TeleoptiPrincipal.Current.Regional.Culture) + 
					")";
			}
		}

		public string ContractTargetTimeHourlyEmployees
		{
			get { return TimeHelper.GetLongHourMinuteTimeString(ContractTargetTime, TeleoptiPrincipal.Current.Regional.Culture); }
		}

		public string TargetDaysOffWithTolerance
		{
			get
			{
				return TargetDaysOff + " (" + (TargetDaysOff - _matrix.SchedulePeriod.Contract.NegativeDayOffTolerance) +
							   " - " + (TargetDaysOff + _matrix.SchedulePeriod.Contract.PositiveDayOffTolerance) +
							   ")";
			}
		}
	}
}
