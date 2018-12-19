using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
    public interface IInitiatorIdentifier
    {
        Guid InitiatorId { get; }
    }
}