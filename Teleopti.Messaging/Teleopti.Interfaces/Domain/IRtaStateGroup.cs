using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IRtaStateGroup : 
		IBelongsToBusinessUnit, 
		IVersioned, 
		IAggregateRoot
    {
        string Name { get; set; }
        bool DefaultStateGroup { get; set; }
        bool Available { get; set; }

		IList<IRtaState> StateCollection { get; }

		IRtaStateGroup AddState(string stateName, string stateCode, Guid platformTypeId);
        IRtaStateGroup AddState(string stateCode, Guid platformTypeId);
        IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state);
    	void DeleteState(IRtaState state);
	    void ClearStates();
    }
}