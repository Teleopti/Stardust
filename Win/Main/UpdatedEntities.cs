using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.Main
{
    public class UpdatedEntities
    {
        public IAggregateRoot UpdatedEntity { get; set; }
        public DomainUpdateType EntityStatus { get; set; }
    }
}
