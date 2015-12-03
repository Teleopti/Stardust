using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds a TimePeriod Template
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-13
    /// </remarks>
    public interface ITemplateTaskPeriod : IAggregateEntity, ITaskOwner, ITaskSource, IPeriodized, ICloneableEntity<ITemplateTaskPeriod>
    {
        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        /// <value>The task.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-13
        /// </remarks>
        ITask Task { get; }

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <value>The campaign.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        ICampaign Campaign { get; }

        /// <summary>
        /// Gets and set the statistic task.
        /// </summary>
        /// <value>The statistic task.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-02-11
        /// </remarks>
		IStatisticTask StatisticTask { get; set; }

        /// <summary>
        /// Gets or sets the aggregated tasks.
        /// </summary>
        /// <value>The aggregated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-02
        /// </remarks>
        double AggregatedTasks { get; set; }

	    IOverrideTask OverrideTask { get; }


	    /// <summary>
        /// Sets the number of tasks.
        /// </summary>
        /// <param name="numberOfTasks">The number of tasks.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 5.12.2007
        /// </remarks>
        void SetTasks(double numberOfTasks);

        /// <summary>
        /// Combines the specified task period.
        /// </summary>
        /// <param name="taskPeriod">The task period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-11
        /// </remarks>
        IList<ITemplateTaskPeriod> Combine(ITemplateTaskPeriod taskPeriod);

        /// <summary>
        /// Splits the specified period length.
        /// </summary>
        /// <param name="periodLength">Length of the period.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// /// </remarks>
        IList<ITemplateTaskPeriodView> Split(TimeSpan periodLength);
    }
}
