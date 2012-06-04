using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
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
		TimeSpan ContractTargetTime { get; set; }
		int TargetDaysOff { get; }
		TimeSpan ContractCurrentTime { get; set; }
		int CurrentDaysOff { get; set; }
	}

	public interface IAgentDisplayData
	{
		IScheduleMatrixPro Matrix { get; }
		TimeSpan MinimumPossibleTime { get; set; }
		TimeSpan MaximumPossibleTime { get; set; }
		int ScheduledAndRestrictionDaysOff { get; set; }
		string Ok { get; }
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
		private readonly Dictionary<AgentRestrictionDisplayRowColumn, string> _warnings;
		public TimePeriod MinMaxTime { get; set; }

		public AgentRestrictionsDisplayRow(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
			State = AgentRestrictionDisplayRowState.NotAvailable;
			_warnings = new Dictionary<AgentRestrictionDisplayRowColumn, string>();
		}

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}

		public void SetWarnings()
		{
			if (!ContractCurrentTime.Equals(ContractTargetTime)) _warnings.Add(AgentRestrictionDisplayRowColumn.ContractTime, UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime);
			if (!CurrentDaysOff.Equals(TargetDaysOff)) _warnings.Add(AgentRestrictionDisplayRowColumn.DaysOffSchedule, UserTexts.Resources.WrongNumberOfDaysOff);
			if (((IAgentDisplayData)this).MinimumPossibleTime > MinMaxTime.EndTime) _warnings.Add(AgentRestrictionDisplayRowColumn.Min, UserTexts.Resources.LowestPossibleWorkTimeIsTooHigh);
			if (((IAgentDisplayData)this).MaximumPossibleTime < MinMaxTime.StartTime) _warnings.Add(AgentRestrictionDisplayRowColumn.Max, UserTexts.Resources.HighestPossibleWorkTimeIsTooLow);
		}

		public string Warning(int index)
		{
			if (_warnings.Count == 0) return null;
			var column = (AgentRestrictionDisplayRowColumn)index;
			string warning;
			_warnings.TryGetValue(column, out warning);
			return warning;
		}

		public int Warnings
		{
			get { return _warnings.Count; }
		}

		public string PeriodType
		{
			get
			{
				switch (_matrix.SchedulePeriod.PeriodType)
				{
					case SchedulePeriodType.Day: return UserTexts.Resources.Day;
					case SchedulePeriodType.Month: return UserTexts.Resources.Month;
					case SchedulePeriodType.Week: return UserTexts.Resources.Week;
					default: return UserTexts.Resources.None;
				}
			}
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
			get { return _matrix.SchedulePeriod.DaysOff(); }
		}

		public string Ok
		{
			get {return _warnings.Count > 0 ? UserTexts.Resources.No : UserTexts.Resources.Yes;}
		}
	}
}
