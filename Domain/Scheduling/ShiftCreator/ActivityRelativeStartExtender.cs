using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// A work shift definition handles activities relative to the shift's start time
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-04
    /// </remarks>
    public class ActivityRelativeStartExtender : ActivityNormalExtender
    {
		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRelativeStartExtender"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="activityLength">Length of the activity.</param>
        /// <param name="relativeStart">The relative start.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public ActivityRelativeStartExtender(IActivity activity,
                                               TimePeriodWithSegment activityLength,
                                               TimePeriodWithSegment relativeStart)
            : base(activity, activityLength, relativeStart)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRelativeStartExtender"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        protected ActivityRelativeStartExtender(){}

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
            DateTime earliestStartTime = projectionStart.Add(ActivityPositionWithSegment.Period.StartTime);
            DateTime latestStartTime = projectionStart.Add(ActivityPositionWithSegment.Period.EndTime);
            if (projectionEnd < latestStartTime)
                latestStartTime = projectionEnd;
            if(latestStartTime < earliestStartTime)
                return null;
            return new DateTimePeriod(earliestStartTime, latestStartTime);
        }


		#endregion Methods 

    }
}