
using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for a view of a ITemplateTaskPeriod
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2009-07-07    
    /// /// </remarks>
    public interface ITemplateTaskPeriodView
    {
        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        DateTimePeriod Period { get; set; }
        /// <summary>
        /// Gets or sets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        TimeSpan AverageTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        TimeSpan AverageAfterTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the campaign task time.
        /// </summary>
        /// <value>The campaign task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        Percent CampaignTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the campaign after task time.
        /// </summary>
        /// <value>The campaign after task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        Percent CampaignAfterTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the total average task time.
        /// </summary>
        /// <value>The total average task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        TimeSpan TotalAverageTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the total average after task time.
        /// </summary>
        /// <value>The total average after task time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        TimeSpan TotalAverageAfterTaskTime { get; set; }
        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        double Tasks { get; set; }
        /// <summary>
        /// Gets or sets the campaign tasks.
        /// </summary>
        /// <value>The campaign tasks.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        double CampaignTasks { get; set; }
        /// <summary>
        /// Gets or sets the total tasks.
        /// </summary>
        /// <value>The total tasks.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        double TotalTasks { get; set; }
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        IWorkloadDay Parent { get; set; }
    }
}
