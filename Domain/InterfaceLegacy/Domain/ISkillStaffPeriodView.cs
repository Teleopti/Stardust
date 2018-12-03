

using System;


namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{

    /// <summary>
    /// 
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2009-07-06    
    /// /// </remarks>
    public interface ISkillStaffPeriodView
    {
        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-06    
        /// /// </remarks>
        DateTimePeriod Period { get; set; }
        /// <summary>
        /// Gets or sets the forecasted distributed demand.
        /// </summary>
        /// <value>The forecasted distributed demand.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-06    
        /// /// </remarks>
        double ForecastedIncomingDemand { get; set; }
        /// <summary>
        /// Gets or sets the calculated resource.
        /// </summary>
        /// <value>The calculated resource.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-06    
        /// /// </remarks>
        double CalculatedResource { get; set; }

        /// <summary>
        /// Gets or sets the F staff.
        /// </summary>
        /// <value>The F staff.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-09-09    
        /// /// </remarks>
        double FStaff { get; set; }

	    double ForecastedTasks { get; set; }
	    Percent EstimatedServiceLevel { get; set; }
	    Percent EstimatedServiceLevelShrinkage { get; set; }
		TimeSpan AverageHandlingTaskTime { get; set; }
    }
}
