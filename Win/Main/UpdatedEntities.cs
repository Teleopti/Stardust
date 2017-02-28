using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main
{
    public class UpdatedEntities
    {
        public IAggregateRoot UpdatedEntity { get; set; }
        public DomainUpdateType EntityStatus { get; set; }
    }
}
