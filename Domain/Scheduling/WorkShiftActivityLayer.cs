using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// WorkShiftActivitylayer class
    /// </summary>
    public class WorkShiftActivityLayer : ActivityLayer, ICloneableEntity<ILayer<IActivity>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkShiftActivityLayer"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public WorkShiftActivityLayer(IActivity activity, DateTimePeriod period) : base(activity, period)
        {
        }
    }
}