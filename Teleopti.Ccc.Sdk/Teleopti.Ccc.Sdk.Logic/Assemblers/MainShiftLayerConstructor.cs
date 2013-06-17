using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class MainShiftLayerConstructor : ILayerConstructor<IMainShiftActivityLayerNew>
    {
        public IMainShiftActivityLayerNew CreateLayer(IActivity activity, DateTimePeriod period)
        {
	        return new MainShiftActivityLayerNew(activity, period);
        }
    }
}