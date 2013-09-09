using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A KPI Key Performance Indicator
    /// </summary>
    public interface IKeyPerformanceIndicator : IAggregateRoot,
        IChangeInfo
    {
        /// <summary>
        /// Gets the resource key.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-17    
        /// </remarks>
        string ResourceKey { get; }

        /// <summary>
        /// Gets the default color of the target.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        Color DefaultBetweenColor { get; }

        /// <summary>
        /// Gets the default color of the max.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        Color DefaultHigherThanMaxColor { get; }

        /// <summary>
        /// Gets the default color of the min.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        Color DefaultLowerThanMinColor { get; }

        /// <summary>
        /// Gets the default max value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        double DefaultMaxValue { get; }

        /// <summary>
        /// Gets the default min value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        double DefaultMinValue { get; }

        /// <summary>
        /// Gets or sets the default target value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        double DefaultTargetValue { get; }

        /// <summary>
        /// Gets the Name
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        string Name { get; }

        ///<summary>
        /// Updated time from the user's perspective
        ///</summary>
        string UpdatedTimeInUserPerspective { get; }

        /// <summary>
        /// Gets the type of the target value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        EnumTargetValueType TargetValueType { get; }

        /// <summary>
        /// Gets the kpi target collection.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        IList<IKpiTarget> KpiTargetCollection { get; }
    }
}