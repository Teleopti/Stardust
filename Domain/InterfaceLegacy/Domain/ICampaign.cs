using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains information about campaign
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-03-04
    /// </remarks>
    public interface ICampaign : IEquatable<ICampaign>
    {
        /// <summary>
        /// Gets the campaign tasks percent.
        /// </summary>
        /// <value>The campaign tasks percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        Percent CampaignTasksPercent { get; }

        /// <summary>
        /// Gets the campaign task time percent.
        /// </summary>
        /// <value>The campaign task time percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        Percent CampaignTaskTimePercent { get; }

        /// <summary>
        /// Gets the campaign after task time percent.
        /// </summary>
        /// <value>The campaign after task time percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        Percent CampaignAfterTaskTimePercent { get; }
    }
}
