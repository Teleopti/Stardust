using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Puts an activity on an absolute position
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-05
    /// </remarks>
    public class ActivityAbsoluteStartExtender : ActivityNormalExtender
    {

		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityAbsoluteStartExtender"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="activityLength">Length of the activity.</param>
        /// <param name="absoluteStart">The absolute start.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public ActivityAbsoluteStartExtender(IActivity activity, 
                                             TimePeriodWithSegment activityLength,
                                             TimePeriodWithSegment absoluteStart) 
                                : base(activity, activityLength, absoluteStart)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityAbsoluteStartExtender"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        protected ActivityAbsoluteStartExtender(){}

		#endregion Constructors 

		#region Methods (1) 


		// Protected Methods (1) 

        /// <summary>
        /// The period available to place the layer in.
        /// </summary>
        /// <param name="templateProjectionPeriod">The template projection period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        protected override DateTimePeriod? PossiblePeriodForActivity(DateTimePeriod templateProjectionPeriod)
        {
            DateTime earliestStart = WorkShift.BaseDate.Add(ActivityPositionWithSegment.Period.StartTime);
            DateTime latestStart = WorkShift.BaseDate.Add(ActivityPositionWithSegment.Period.EndTime);
            return new DateTimePeriod(earliestStart, latestStart);
        }


		#endregion Methods 

    }
}