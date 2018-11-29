using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// DateTimePeriod + SkillstaffSegment
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-09
    /// </remarks>
    public class SkillStaffSegmentPeriod : Layer<ISkillStaffSegment>, ISkillStaffSegmentPeriod
    {
        private readonly ISkillStaffPeriod _belongsTo;
        private readonly ISkillStaffPeriod _belongsToY;
        private double _bookedResource65;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillStaffSegmentPeriod"/> class.
        /// </summary>
        /// <param name="belongsToX">The belongs to X.</param>
        /// <param name="belongsToY">The belongs to Y.</param>
        /// <param name="skillStaffSegment">The skill staff segment.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        public SkillStaffSegmentPeriod( ISkillStaffPeriod belongsToX, 
                                        ISkillStaffPeriod belongsToY,
                                        ISkillStaffSegment skillStaffSegment, 
                                        DateTimePeriod period)
            : base(skillStaffSegment, period)
        {
            _belongsTo = belongsToX;
            _belongsToY = belongsToY;
        }

        /// <summary>
        /// Gets the belongs to.
        /// </summary>
        /// <value>The belongs to.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        public ISkillStaffPeriod BelongsTo
        {
            get { return _belongsTo; }
        }

        /// <summary>
        /// Gets the belongs to Y.
        /// </summary>
        /// <value>The belongs to Y.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-04
        /// </remarks>
        public ISkillStaffPeriod BelongsToY
        {
            get { return _belongsToY; }
        }

        /// <summary>
        /// Gets or sets the booked resource65.
        /// </summary>
        /// <value>The booked resource65.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-05
        /// </remarks>
        public double BookedResource65
        {
            get
            {
                return _bookedResource65;
            }
            set
            {
                _bookedResource65 = value;
            }
        }

        /// <summary>
        /// Anders defined FStaff value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-09
        /// </remarks>
        public double FStaff()
        {
            double incommingDifference = BelongsTo.IncomingDifference;
            if (incommingDifference <= 0)
            {
                return Math.Round(BookedResource65 - (incommingDifference / BelongsTo.SortedSegmentCollection.Count), 3);
            }

            double divider = BelongsTo.Payload.BookedAgainstIncomingDemand65;
            if(divider > 0)
            {
                return Math.Round((BookedResource65/divider) *
                             BelongsTo.Payload.ForecastedIncomingDemand, 3);
            }

            return 0;
        }

        public override string ToString()
        {
            return base.ToString() + ", BelongsTo " + BelongsTo.Period;
        }
    }
}
