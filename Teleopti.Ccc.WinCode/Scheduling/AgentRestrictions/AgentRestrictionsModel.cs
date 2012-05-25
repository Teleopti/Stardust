﻿using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsModel
	{
		IList<AgentRestrictionsDisplayRow> DisplayRows { get; }
		AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex);
		void LoadData(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator);
	}

	public class AgentRestrictionsModel : IAgentRestrictionsModel
	{
		private IList<AgentRestrictionsDisplayRow> _displayRows;


		public AgentRestrictionsModel()
		{
			_displayRows = new List<AgentRestrictionsDisplayRow>();
		}

		public IList<AgentRestrictionsDisplayRow> DisplayRows { get { return _displayRows; } }

		public void LoadData(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator)
		{
			if(agentRestrictionsDisplayRowCreator == null) throw new ArgumentNullException("agentRestrictionsDisplayRowCreator");

			_displayRows = agentRestrictionsDisplayRowCreator.Create();
		}

		public AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex)
		{
			if(rowIndex < int.MinValue + 2) throw new ArgumentOutOfRangeException("rowIndex");
			return _displayRows[rowIndex - 1 - 1];
		}
	}
}
