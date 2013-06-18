using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class MainShiftLayerConstructor : ILayerConstructor<IMainShiftLayer>
    {
        public IMainShiftLayer CreateLayer(IActivity activity, DateTimePeriod period)
        {
	        return new MainShiftLayer(activity, period);
        }
    }
}