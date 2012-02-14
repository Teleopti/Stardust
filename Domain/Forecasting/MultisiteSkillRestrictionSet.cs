using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// A set of restrictions for domain object MultisiteSkill
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public class MultisiteSkillRestrictionSet : RestrictionSet<ISkill>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteSkillRestrictionSet"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public MultisiteSkillRestrictionSet() : base(new List<IRestriction<ISkill>>{ new MinMaxAgents(), new MinMaxOccupancy(), new DistributionPercentage() })
        {
        }

        private static MultisiteSkillRestrictionSet _currentRestrictionSet;

        /// <summary>
        /// Gets the current restriction set.
        /// </summary>
        /// <value>The current restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        public static MultisiteSkillRestrictionSet CurrentRestrictionSet
        {
            get
            {
                if (_currentRestrictionSet == null)
                    _currentRestrictionSet = new MultisiteSkillRestrictionSet();
                return _currentRestrictionSet;
            }
        }
    }
}