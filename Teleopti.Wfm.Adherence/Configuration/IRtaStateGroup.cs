using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public interface IRtaStateGroup :
		IBelongsToBusinessUnitId,
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