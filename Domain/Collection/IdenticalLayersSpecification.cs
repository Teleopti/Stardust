using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Collection
{
    public class IdenticalLayersSpecification : Specification<IVisualLayerCollection>
    {
        private readonly IVisualLayerCollection _collectionToCompare;

        public IdenticalLayersSpecification(IVisualLayerCollection collectionToCompare)
        {
            _collectionToCompare = collectionToCompare;
        }

        public override bool IsSatisfiedBy(IVisualLayerCollection obj)
        {
            if(obj.Count() != _collectionToCompare.Count())
                return false;

            IEnumerable<IVisualLayer> objEnum = obj.CopyEnumerable<IVisualLayer>();
            List<IVisualLayer> objList = new List<IVisualLayer>(objEnum);

            IEnumerable<IVisualLayer> compEnum = _collectionToCompare.CopyEnumerable<IVisualLayer>();
            List<IVisualLayer> compList = new List<IVisualLayer>(compEnum);

            for (int i = 0; i < obj.Count(); i++)
            {
                IVisualLayer objLayer = objList[i];
                IVisualLayer compLayer = compList[i];

                if(!objLayer.Period.Equals(compLayer.Period))
                    return false;

                if(!objLayer.Payload.Equals(compLayer.Payload))
                    return false;
            }

            return true;
        }
    }
}