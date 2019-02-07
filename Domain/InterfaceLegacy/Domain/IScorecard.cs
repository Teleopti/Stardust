using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Holds values for one Scorecard, the Name and the Key Performance Indicators that are used in the Scorecard
    ///</summary>
    public interface IScorecard : IAggregateRoot, IChangeInfo,
                                    IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-17    
        /// </remarks>
        IScorecardPeriod Period { get; set; }

        /// <summary>
        /// Gets the key performance indicator collection.
        /// </summary>
        /// <value>The key performance indicator collection.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        ReadOnlyCollection<IKeyPerformanceIndicator> KeyPerformanceIndicatorCollection { get; }

        /// <summary>
        /// Adds the kpi to the Scorecard.
        /// </summary>
        /// <param name="kpi">The kpi.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        void AddKpi(IKeyPerformanceIndicator kpi);

        /// <summary>
        /// Removes the kpi from the Scorecard.
        /// </summary>
        /// <param name="kpi">The kpi.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        void RemoveKpi(IKeyPerformanceIndicator kpi);
    }
}