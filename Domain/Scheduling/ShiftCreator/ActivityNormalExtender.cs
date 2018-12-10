using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Base class for non auto positioned activity extenders
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-27
    /// </remarks>
    public abstract class ActivityNormalExtender : ActivityExtender, IActivityNormalExtender
    {
        private TimePeriodWithSegment _activityPositionWithSegment;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityNormalExtender"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="activityLengthWithSegment">The activity length with segment.</param>
        /// <param name="activityPositionWithSegment">The activity position with segment.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        protected ActivityNormalExtender(IActivity activity, 
                            TimePeriodWithSegment activityLengthWithSegment,
                            TimePeriodWithSegment activityPositionWithSegment)
            : base(activity, activityLengthWithSegment)
        {
            _activityPositionWithSegment = activityPositionWithSegment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityNormalExtender"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-28
        /// </remarks>
        protected ActivityNormalExtender(){}
		
        /// <summary>
        /// Gets or sets the activity position with segment.
        /// </summary>
        /// <value>The activity position with segment.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual TimePeriodWithSegment ActivityPositionWithSegment
        {
            get { return _activityPositionWithSegment; }
            set { _activityPositionWithSegment = value; }
        }
		
        /// <summary>
        /// Gets the possible length of the activity.
        /// </summary>
        /// <returns></returns>
        /// <value>The length of the activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        public override TimeSpan ExtendMaximum()
        {
            return ActivityLengthWithSegment.Period.EndTime;
        }

        /// <summary>
        /// Processes the shift. Generates new workshifts.
        /// </summary>
        /// <param name="shift">The shift.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-03
        /// </remarks>
        public override IList<IWorkShift> ReplaceWithNewShifts(IWorkShift shift)
        {
            InParameter.NotNull(nameof(shift), shift);

            DateTimePeriod? projPeriod = shift.LayerCollection.Period();
            DateTimePeriod? actPeriod = PossiblePeriodForActivity(projPeriod.Value);
            if(actPeriod != null)
                return createReturnWorkShifts(shift, projPeriod.Value, actPeriod.Value);
            return new List<IWorkShift>();
        }
		
        /// <summary>
        /// The period available to place the layer in.
        /// </summary>
        /// <param name="templateProjectionPeriod">The template projection period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        protected abstract DateTimePeriod? PossiblePeriodForActivity(DateTimePeriod templateProjectionPeriod);
		
        private IWorkShift createNewShift(IWorkShift template, DateTime layerStartTime, DateTime layerEndTime)
        {
            IWorkShift newShift = (WorkShift)template.Clone();
            newShift.LayerCollection.Add(
                new WorkShiftActivityLayer(ExtendWithActivity,
                                           new DateTimePeriod(layerStartTime, layerEndTime)));
            return newShift;
        }

        private IList<IWorkShift> createReturnWorkShifts(IWorkShift shift, DateTimePeriod projPeriod, DateTimePeriod actPeriod)
        {
            IList<IWorkShift> retColl = new List<IWorkShift>();
            for (TimeSpan actLength = ActivityLengthWithSegment.Period.StartTime;
                 actLength <= ActivityLengthWithSegment.Period.EndTime;
                 actLength = actLength.Add(ActivityLengthWithSegment.Segment))
            {

                for (DateTime layerStart = actPeriod.StartDateTime;
                     layerStart <= actPeriod.EndDateTime;
                     layerStart = layerStart.Add(_activityPositionWithSegment.Segment))
                {
                    //todo: "knipsa av" lagret istället för att säga nej? fråga anders....
                    DateTime layerEnd = layerStart.Add(actLength);
                    if (projPeriod.StartDateTime <= layerStart && projPeriod.EndDateTime >= layerEnd)
                    {
                        IWorkShift newShift = createNewShift(shift, layerStart, layerEnd);
                        retColl.Add(newShift);
                    }
                }
            }
            return retColl;
        }
    }
}
