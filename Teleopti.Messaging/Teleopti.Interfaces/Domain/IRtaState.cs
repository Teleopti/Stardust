using System;

namespace Teleopti.Interfaces.Domain
{
    public interface IRtaState : IAggregateEntity, ICloneableEntity<IRtaState>
    {
		IBusinessUnit BusinessUnit { get; }
        string Name { get; set; }
        string StateCode { get; set; }
        IRtaStateGroup StateGroup { get; }
        Guid PlatformTypeId { get; }
    }
}