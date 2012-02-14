
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class OneSpecificLayerSpecification : Specification<IVisualLayerCollection>
    {
        private readonly IPayload _payload;

        internal OneSpecificLayerSpecification(IPayload payload)
        {
            InParameter.NotNull("payload", payload);
            _payload = payload;
        }

        public override bool IsSatisfiedBy(IVisualLayerCollection obj)
        {
            return obj.Count()==1 && obj.First().Payload.Equals(_payload);
        }
    }
}
