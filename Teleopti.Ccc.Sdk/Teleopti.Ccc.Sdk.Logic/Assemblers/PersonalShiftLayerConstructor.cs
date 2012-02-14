using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonalShiftLayerConstructor : ILayerConstructor<IPersonalShiftActivityLayer>
    {
        public IPersonalShiftActivityLayer CreateLayer(IActivity activity, DateTimePeriod period)
        {
            return new PersonalShiftActivityLayer(activity, period);
        }
    }
}