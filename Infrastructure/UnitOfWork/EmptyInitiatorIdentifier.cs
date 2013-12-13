using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class EmptyInitiatorIdentifier : IInitiatorIdentifier
    {
        public EmptyInitiatorIdentifier()
        {
            InstanceId = Guid.Empty;
        }

        public Guid InstanceId { get; private set; }
    }
}