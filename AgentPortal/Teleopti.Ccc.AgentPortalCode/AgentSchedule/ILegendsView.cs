using System.Collections;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    /// <summary>
    /// Interface for LegendsView
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2010-07-02
    /// </remarks>
    public interface ILegendsView
    {
        /// <summary>
        /// Gets or sets the absence data source.
        /// </summary>
        /// <value>The absence data source.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-07-02
        /// </remarks>
        ArrayList AbsenceDataSource { get; set; }

        /// <summary>
        /// Gets or sets the activity data source.
        /// </summary>
        /// <value>The activity data source.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-07-02
        /// </remarks>
        ArrayList ActivityDataSource { get;  set; }

        /// <summary>
        /// Gets or sets the height of the absence.
        /// </summary>
        /// <value>The height of the absence.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-07-02
        /// </remarks>
        int AbsenceHeight { get; set; }

        /// <summary>
        /// Gets or sets the height of the activity.
        /// </summary>
        /// <value>The height of the activity.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-07-02
        /// </remarks>
        int ActivityHeight { get; set; }
    }
}