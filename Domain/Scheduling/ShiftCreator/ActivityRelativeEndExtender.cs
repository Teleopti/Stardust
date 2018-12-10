using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Puts an activity layer on a relative end position
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-06
    /// </remarks>
    public class ActivityRelativeEndExtender : ActivityNormalExtender
    {

		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRelativeEndExtender"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="activityLength">Length of the activity.</param>
        /// <param name="relativeEnd">Length of the period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-06
        /// </remarks>
        public ActivityRelativeEndExtender(IActivity activity, 
                                    TimePeriodWithSegment activityLength, 
                                    TimePeriodWithSegment relativeEnd)
            : base(activity, activityLength, relativeEnd)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRelativeEndExtender"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        protected ActivityRelativeEndExtender(){}

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
            DateTime projectionStart = templateProjectionPeriod.StartDateTime;
            DateTime projectionEnd = templateProjectionPeriod.EndDateTime;
            DateTime earliestStartTime = projectionEnd.Subtract(ActivityPositionWithSegment.Period.EndTime);
            DateTime latestStartTime = projectionEnd.Subtract(ActivityPositionWithSegment.Period.StartTime);
            if (projectionStart > earliestStartTime)
                earliestStartTime = projectionStart;
            if (latestStartTime < earliestStartTime)
                return null;
            return new DateTimePeriod(earliestStartTime, latestStartTime);
        }


		#endregion Methods 

    }
}