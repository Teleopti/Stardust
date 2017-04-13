using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
    public class UpdatedEntities
    {
        public IAggregateRoot UpdatedEntity { get; set; }
        public DomainUpdateType EntityStatus { get; set; }
    }
}
