using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class EmptyInitiatorIdentifier : IInitiatorIdentifier
    {
        public EmptyInitiatorIdentifier()
        {
            InitiatorId = Guid.Empty;
        }

        public Guid InitiatorId { get; private set; }
    }
}