using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public interface IAllLayersAreInWorkTimeSpecification
    {
        bool IsSatisfiedBy(IFilteredVisualLayerCollection layers);
    }

    public class AllLayersAreInWorkTimeSpecification : Specification<IFilteredVisualLayerCollection>, IAllLayersAreInWorkTimeSpecification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override bool IsSatisfiedBy(IFilteredVisualLayerCollection layers)
        {
            foreach (var layer in layers)
            {
                var activity = layer.Payload as IActivity;
                if (activity != null && !activity.InWorkTime)
                    return false;
                IAbsence absence = null;
                if (activity == null)
                {
                    absence = layer.Payload as IAbsence;
                    if (absence != null && !absence.InWorkTime)
                        return false;
                }

                if (activity == null && absence == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}