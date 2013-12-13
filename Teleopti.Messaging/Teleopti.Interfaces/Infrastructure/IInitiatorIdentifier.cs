using System;

namespace Teleopti.Interfaces.Infrastructure
{
    public interface IInitiatorIdentifier
    {
        Guid InstanceId { get; }
    }
}