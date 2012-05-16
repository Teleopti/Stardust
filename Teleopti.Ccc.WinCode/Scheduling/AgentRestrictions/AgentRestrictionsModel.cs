using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsModel
	{
		IList<AgentRestrictionsDisplayRow> DisplayRows { get; }
		AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex);
		void LoadData();
	}

	public class AgentRestrictionsModel : IAgentRestrictionsModel
	{
		private readonly IList<AgentRestrictionsDisplayRow> _displayRows;


		public AgentRestrictionsModel()
		{
			_displayRows = new List<AgentRestrictionsDisplayRow>();
		}

		public IList<AgentRestrictionsDisplayRow> DisplayRows { get { return _displayRows; } }

		public void LoadData()
		{
			_displayRows.Add(new AgentRestrictionsDisplayRow(null));
			_displayRows.Add(new AgentRestrictionsDisplayRow(null));
			_displayRows.Add(new AgentRestrictionsDisplayRow(null));
		}

		public AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex)
		{
			if(rowIndex < int.MinValue + 2) throw new ArgumentOutOfRangeException("rowIndex");
			return _displayRows[rowIndex - 1 - 1];
		}
	}
}
