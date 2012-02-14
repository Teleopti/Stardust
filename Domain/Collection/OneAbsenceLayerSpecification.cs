using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class OneAbsenceLayerSpecification : Specification<IVisualLayerCollection>
    {
        internal OneAbsenceLayerSpecification() { }

        public override bool IsSatisfiedBy(IVisualLayerCollection obj)
        {
            var firstItem = obj.FirstOrDefault();

            return firstItem != null &&
                   firstItem.Payload is IAbsence &&
                   obj.All(l => l.Payload.Equals(firstItem.Payload));
        }
    }
}
