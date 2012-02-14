using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for the Generator of Work Shift
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-03-06    
    /// /// </remarks>
    public interface IWorkShiftTemplateGenerator : ICloneableEntity<IWorkShiftTemplateGenerator>
    {
        /// <summary>
        /// Generates a List of Work Shifts.
        /// </summary>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-03-06    
        /// /// </remarks>
        IList<IWorkShift> Generate();

        /// <summary>
        /// Gets or sets the base activity.
        /// </summary>
        /// <value>The base activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        IActivity BaseActivity { get; set; }

        /// <summary>
        /// Gets or sets the possible start period.
        /// </summary>
        /// <value>The start period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        TimePeriodWithSegment StartPeriod { get; set; }

        /// <summary>
        /// Gets or sets the possible end period.
        /// </summary>
        /// <value>The end period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        TimePeriodWithSegment EndPeriod { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-10
        /// </remarks>
        IShiftCategory Category { get; set; }

    }
}