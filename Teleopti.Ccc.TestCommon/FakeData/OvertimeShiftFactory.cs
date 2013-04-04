using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for OvertimeShift domain object
    /// </summary>
    public static class OvertimeShiftFactory
    {
        /// <summary>
        /// Creates the overtime shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <param name="definitionSet">The definition set.</param>
        /// <param name="assignment">The assignment.</param>
        /// <returns></returns>
        public static OvertimeShift CreateOvertimeShift(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet, IPersonAssignment assignment)
        {
            OvertimeShift shift = new OvertimeShiftForTest();
            assignment.AddOvertimeShift(shift);
            var layer = new OvertimeShiftActivityLayer(activity, period, definitionSet);
            shift.LayerCollection.Add(layer);
            return shift;
        }
    }

	public class OvertimeShiftForTest : OvertimeShift, ILayerCollectionOwner<IActivity>
    {
		public new void OnAdd(ILayer<IActivity> layer)
       {
       }
    }
}