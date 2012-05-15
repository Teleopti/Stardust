namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public enum AgentRestrictionDisplayRowColumn
	{
		AgentName,
		Warnings,
		Type,
		From,
		To,
		ContractTargetTime,
		DaysOff,
		ContractTime,
		DaysOffSchedule,
		Min,
		Max,
		DaysOffScheduleRestrictions,
		Ok,
		None
	}

	public class AgentRestrictionsDisplayRowColumnMapper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public AgentRestrictionDisplayRowColumn ColumnFromIndex(int index)
		{
			if (index == 0) return AgentRestrictionDisplayRowColumn.AgentName;
			if (index == 1) return AgentRestrictionDisplayRowColumn.Warnings;
			if (index == 2) return AgentRestrictionDisplayRowColumn.Type;
			if (index == 3) return AgentRestrictionDisplayRowColumn.From;
			if (index == 4) return AgentRestrictionDisplayRowColumn.To;
			if (index == 5) return AgentRestrictionDisplayRowColumn.ContractTargetTime;
			if (index == 6) return AgentRestrictionDisplayRowColumn.DaysOff;
			if (index == 7) return AgentRestrictionDisplayRowColumn.ContractTime;
			if (index == 8) return AgentRestrictionDisplayRowColumn.DaysOffSchedule;
			if (index == 9) return AgentRestrictionDisplayRowColumn.Min;
			if (index == 10) return AgentRestrictionDisplayRowColumn.Max;
			if (index == 11) return AgentRestrictionDisplayRowColumn.DaysOffScheduleRestrictions;
			if (index == 12) return AgentRestrictionDisplayRowColumn.Ok;

			return AgentRestrictionDisplayRowColumn.None;
		}
	}
}
