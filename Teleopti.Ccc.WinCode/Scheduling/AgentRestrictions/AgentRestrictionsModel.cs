using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsModel
	{
		IList<AgentRestrictionsDisplayRow> DisplayRows { get; }	
	}

	public class AgentRestrictionsModel : IAgentRestrictionsModel
	{
		private IList<AgentRestrictionsDisplayRow> _displayRows;


		public AgentRestrictionsModel()
		{
			_displayRows = new List<AgentRestrictionsDisplayRow>();
		}

		public IList<AgentRestrictionsDisplayRow> DisplayRows { get { return _displayRows; } }	
	}
}
