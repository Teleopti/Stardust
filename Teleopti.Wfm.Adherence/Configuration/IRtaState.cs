using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
    public interface IRtaState : IAggregateEntity, ICloneableEntity<IRtaState>
    {
		//Guid? BusinessUnit { get; }
        string Name { get; set; }
        string StateCode { get; set; }
        IRtaStateGroup StateGroup { get; }
    }
}