using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class EmptyMessageBrokerIdentifier : IMessageBrokerIdentifier
    {
        public EmptyMessageBrokerIdentifier()
        {
            InstanceId = Guid.Empty;
        }

        public Guid InstanceId { get; private set; }
    }
}