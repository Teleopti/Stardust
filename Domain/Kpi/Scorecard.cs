using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Kpi
{
    /// <summary>
    /// Holds values for one Scorecard, the Name and the Key Performance Indicators that are used in the Scorecard
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-15    
    /// </remarks>
    public class Scorecard : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IScorecard
    {
        private string _name;
        private readonly IList<IKeyPerformanceIndicator> _keyPerformanceIndicatorCollection = new List<IKeyPerformanceIndicator>();
        private int _period;

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-17    
        /// </remarks>
        public virtual IScorecardPeriod Period
        {
            get
            {
                return ScorecardPeriodService.ScorecardPeriodList().FirstOrDefault(p => p.Id == _period);
            }
            set { _period = value.Id; }
        }

        /// <summary>
        /// Gets the key performance indicator collection.
        /// </summary>
        /// <value>The key performance indicator collection.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        public virtual ReadOnlyCollection<IKeyPerformanceIndicator> KeyPerformanceIndicatorCollection => new ReadOnlyCollection<IKeyPerformanceIndicator>(_keyPerformanceIndicatorCollection);

	    /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Adds the kpi to the Scorecard.
        /// </summary>
        /// <param name="kpi">The kpi.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        public virtual void AddKpi(IKeyPerformanceIndicator kpi)
        {
            InParameter.NotNull(nameof(kpi), kpi);
            _keyPerformanceIndicatorCollection.Add(kpi);
        }

        /// <summary>
        /// Removes the kpi from the Scorecard.
        /// </summary>
        /// <param name="kpi">The kpi.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-15    
        /// </remarks>
        public virtual void RemoveKpi(IKeyPerformanceIndicator kpi)
        {
            InParameter.NotNull(nameof(kpi), kpi);
            _keyPerformanceIndicatorCollection.Remove(kpi);
        }
    }
}
