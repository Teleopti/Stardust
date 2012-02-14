using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class HasLayerWithPeriodAndActivityThatIsEqualTo : Specification<IVisualLayer>
    {
        private readonly IList<IVisualLayer> _layers;

        public HasLayerWithPeriodAndActivityThatIsEqualTo(IList<IVisualLayer> layers)
        {
            _layers = layers;
        }

        public override bool IsSatisfiedBy(IVisualLayer obj)
        {
            foreach (IVisualLayer layer in _layers)
            {
                if (layer.Payload.Equals(obj.Payload) && layer.Period.Equals(obj.Period)) return true;
            }

            return false;
        }
    }
}
