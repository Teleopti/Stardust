using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class MainShiftLayerConstructor : ILayerConstructor<IMainShiftActivityLayer>
    {
        public IMainShiftActivityLayer CreateLayer(IActivity activity, DateTimePeriod period)
        {
            return new MainShiftActivityLayer(activity,period);
        }
    }
}