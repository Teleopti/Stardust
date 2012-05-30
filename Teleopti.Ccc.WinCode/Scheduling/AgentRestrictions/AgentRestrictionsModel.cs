using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsModel
	{

		IList<AgentRestrictionsDisplayRow> DisplayRows { get; }
		AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex);
		//void LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator);
		IList<AgentRestrictionsDisplayRow> LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator, IList<IPerson> persons);
	}

	public class AgentRestrictionsModel : IAgentRestrictionsModel
	{
		private IList<AgentRestrictionsDisplayRow> _displayRows;
		

		public AgentRestrictionsModel()
		{
			_displayRows = new List<AgentRestrictionsDisplayRow>();		
		}

		public IList<AgentRestrictionsDisplayRow> DisplayRows { get { return _displayRows; } }

		public AgentRestrictionsDisplayRow DisplayRowFromRowIndex(int rowIndex)
		{
			if (rowIndex < int.MinValue + 2) throw new ArgumentOutOfRangeException("rowIndex");
			return _displayRows[rowIndex - 1 - 1];
		}

		//public void LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator)
		//{
		//    if(agentRestrictionsDisplayRowCreator == null) throw new ArgumentNullException("agentRestrictionsDisplayRowCreator");

		//    var rows = agentRestrictionsDisplayRowCreator.Create();

		//    foreach (var agentRestrictionsDisplayRow in rows)
		//    {
		//        _displayRows.Add(agentRestrictionsDisplayRow);
		//    }	
		//    //_displayRows = agentRestrictionsDisplayRowCreator.Create();	
		//}

		public IList<AgentRestrictionsDisplayRow> LoadDisplayRows(IAgentRestrictionsDisplayRowCreator agentRestrictionsDisplayRowCreator, IList<IPerson> persons)
		{
			if(agentRestrictionsDisplayRowCreator == null) throw new ArgumentNullException("agentRestrictionsDisplayRowCreator");

			var rows = agentRestrictionsDisplayRowCreator.Create(persons);

			foreach (var agentRestrictionsDisplayRow in rows)
			{
				_displayRows.Add(agentRestrictionsDisplayRow);
			}

			return rows;
		}
	}
}
