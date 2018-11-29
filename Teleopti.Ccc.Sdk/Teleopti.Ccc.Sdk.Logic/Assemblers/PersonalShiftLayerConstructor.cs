using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonalShiftLayerConstructor : ILayerConstructor<PersonalShiftLayer>
    {
        public PersonalShiftLayer CreateLayer(IActivity activity, DateTimePeriod period)
        {
            return new PersonalShiftLayer(activity, period);
        }
    }
}