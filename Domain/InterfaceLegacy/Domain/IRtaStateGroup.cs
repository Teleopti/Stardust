using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IRtaStateGroup :
		IBelongsToBusinessUnit,
		IVersioned,
		IAggregateRootWithEvents
	{
		string Name { get; set; }
		bool DefaultStateGroup { get; set; }
		bool Available { get; set; }

		IList<IRtaState> StateCollection { get; }

		bool IsLogOutState { get; set; }

		IRtaStateGroup AddState(string stateCode, string stateName);
		IRtaStateGroup AddState(string stateCode);
		IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state);
		void DeleteState(IRtaState state);
		void ClearStates();
	}
}