using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class WorkShiftContainsMasterActivitySpecification : Specification<ILayerCollectionOwner<IActivity>>
    {
		public override bool IsSatisfiedBy(ILayerCollectionOwner<IActivity> obj)
		{
			if (obj == null)
				return false;

			return obj.LayerCollection.Select(layer => layer.Payload).OfType<IMasterActivity>().Any();
		}
    }
}