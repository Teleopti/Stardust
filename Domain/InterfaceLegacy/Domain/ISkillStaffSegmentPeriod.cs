namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Segmented staffing information for skill intraday
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillStaffSegmentPeriod : ILayer<ISkillStaffSegment>
    {
        /// <summary>
        /// Gets the belongs to.
        /// </summary>
        /// <value>The belongs to.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        ISkillStaffPeriod BelongsTo { get; }

        /// <summary>
        /// Gets the belongs to Y.
        /// </summary>
        /// <value>The belongs to Y.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-04
        /// </remarks>
        ISkillStaffPeriod BelongsToY { get; }

        /// <summary>
        /// Gets or sets the booked resource65.
        /// </summary>
        /// <value>The booked resource65.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-05
        /// </remarks>
        double BookedResource65 { get; set; }

        /// <summary>
        /// Anders defined FStaff value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-09
        /// </remarks>
        double FStaff();

    }
}