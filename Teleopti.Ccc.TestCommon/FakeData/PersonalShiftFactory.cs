using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for PersonalShift domain object
    /// </summary>
    public static class PersonalShiftFactory
    {
        /// <summary>
        /// Creates the personal shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static PersonalShift CreatePersonalShift(IActivity activity, DateTimePeriod period)
        {
            PersonalShift shift = new PersonalShift();
            PersonalShiftActivityLayer layer = new PersonalShiftActivityLayer(activity, period);
            shift.LayerCollection.Add(layer);
            return shift;
        }
    }
}