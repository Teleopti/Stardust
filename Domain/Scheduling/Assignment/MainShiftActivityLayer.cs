using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// MainshiftActivitylayer class
    /// </summary>
    public class MainShiftActivityLayer : ActivityLayer, IMainShiftActivityLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainShiftActivityLayer"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public MainShiftActivityLayer(IActivity activity, DateTimePeriod period)
            : base(activity, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainShiftActivityLayer"/> class.
        /// </summary>
        protected MainShiftActivityLayer()
        {
        }
    }
}