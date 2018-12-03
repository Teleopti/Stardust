using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class MainShiftLayerConstructor : ILayerConstructor<MainShiftLayer>
    {
        public MainShiftLayer CreateLayer(IActivity activity, DateTimePeriod period)
        {
	        return new MainShiftLayer(activity, period);
        }
    }
}