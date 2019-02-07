using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public interface IRtaStateGroup :
		IFilterOnBusinessUnitId,
		IVersioned,
		IAggregateRoot, 
		IPublishEvents
	{
		string Name { get; set; }
		bool DefaultStateGroup { get; set; }
		bool Available { get; set; }

		IList<IRtaState> StateCollection { get; }

		bool IsLogOutState { get; set; }

		void AddState(string stateCode, string stateName);
		void AddState(string stateCode);
		IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state);
		void DeleteState(IRtaState state);
		void ClearStates();
	}
}