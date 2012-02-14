using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// A set of restrictions for domain object MultisiteDay
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public class MultisiteDayRestrictionSet : RestrictionSet<IMultisiteDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDayRestrictionSet"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public MultisiteDayRestrictionSet()
            : base(new List<IRestriction<IMultisiteDay>>
        {
            new DistributionPercentage()
        })
    {
    }

        private static MultisiteDayRestrictionSet _currentRestrictionSet;

        /// <summary>
        /// Gets the current restriction set.
        /// </summary>
        /// <value>The current restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        public static MultisiteDayRestrictionSet CurrentRestrictionSet
        {
            get
            {
                if (_currentRestrictionSet == null)
                    _currentRestrictionSet = new MultisiteDayRestrictionSet();
                return _currentRestrictionSet;
            }
        }
    }
}