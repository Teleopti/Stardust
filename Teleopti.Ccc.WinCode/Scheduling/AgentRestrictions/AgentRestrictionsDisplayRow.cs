using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
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
	}

	public interface IAgentDisplayData
	{
		IScheduleMatrixPro Matrix { get; }
		TimeSpan MinimumPossibleTime { get; set; }
		TimeSpan MaximumPossibleTime { get; set; }
		int ScheduledAndRestrictionDaysOff { get; set; }
	}

	public sealed class AgentRestrictionsDisplayRow : IAgentRestrictionsDisplayRow, IAgentDisplayData
	{
		private readonly IScheduleMatrixPro _matrix;
		TimeSpan IAgentDisplayData.MinimumPossibleTime { get; set; }
		TimeSpan IAgentDisplayData.MaximumPossibleTime { get; set; }
		int IAgentDisplayData.ScheduledAndRestrictionDaysOff { get; set; }
		public AgentRestrictionDisplayRowState State { get; set; }
		public string AgentName { get; set; }
		private readonly Dictionary<AgentRestrictionDisplayRowColumn, string> _warnings;
		private readonly AgentRestrictionsDisplayRowColumnMapper _columnMapper;

		public AgentRestrictionsDisplayRow(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
			State = AgentRestrictionDisplayRowState.NotAvailable;
			_warnings = new Dictionary<AgentRestrictionDisplayRowColumn, string>();
			_columnMapper = new AgentRestrictionsDisplayRowColumnMapper();
		}

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}

		public void SetWarning(AgentRestrictionDisplayRowColumn agentRestrictionDisplayRowColumn, string warning)
		{
			_warnings.Add(agentRestrictionDisplayRowColumn, warning);	
		}

		public string Warning(int index)
		{
			if (_warnings.Count == 0) return null;
			var column = _columnMapper.ColumnFromIndex(index);
			string warning;
			_warnings.TryGetValue(column, out warning);
			return warning;
		}

		public int Warnings
		{
			get { return _warnings.Count; }
		}
	}
}
