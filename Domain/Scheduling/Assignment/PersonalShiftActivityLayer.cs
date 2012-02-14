using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// PersonalShiftActivityLayer class
    /// </summary>
    public class PersonalShiftActivityLayer : ActivityLayer, IPersonalShiftActivityLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalShiftActivityLayer"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public PersonalShiftActivityLayer(IActivity activity, DateTimePeriod period) : base(activity, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalShiftActivityLayer"/> class.
        /// </summary>
        protected PersonalShiftActivityLayer()
        {
        }
    }
}