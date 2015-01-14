using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AgentStateInfo
	{
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<AgentState> _currentState;

		public AgentStateInfo(
			Func<AgentState> makePreviousState,
			Func<AgentState> makeCurrentState 
			)
		{
			_previousState = new Lazy<AgentState>(makePreviousState);
			_currentState = new Lazy<AgentState>(makeCurrentState);
		}

		public AgentState CurrentState()
		{
			return _currentState.Value;
		}

		public AgentState PreviousState()
		{
			return _previousState.Value;
		}
	}
}