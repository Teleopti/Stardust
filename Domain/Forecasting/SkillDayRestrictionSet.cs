using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// A set of restrictions for domain object SkillDay
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public class SkillDayRestrictionSet : RestrictionSet<ISkillDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayRestrictionSet"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public SkillDayRestrictionSet() : base(new List<IRestriction<ISkillDay>>()
        {
            new MinMaxAgents(),
            new MinMaxOccupancy()
        })
        {
        }

        private static IRestrictionSet<ISkillDay> _currentRestrictionSet;

        /// <summary>
        /// Gets the current restriction set.
        /// </summary>
        /// <value>The current restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        public static IRestrictionSet<ISkillDay> CurrentRestrictionSet
        {
            get{
                if (_currentRestrictionSet == null)
                    _currentRestrictionSet = new SkillDayRestrictionSet();
                return _currentRestrictionSet;
            }
        }
    }
}