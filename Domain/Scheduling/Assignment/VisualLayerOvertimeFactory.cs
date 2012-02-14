using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class VisualLayerOvertimeFactory : VisualLayerFactory
    {
        public override IVisualLayer CreateShiftSetupLayer(IActivity activity, DateTimePeriod period)
        {
            VisualLayer ret = new VisualLayer(activity, period, activity);
            ret.DefinitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);
            return ret;
        }

    }
}
