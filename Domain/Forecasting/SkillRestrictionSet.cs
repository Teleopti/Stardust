using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{


    /// <summary>
    /// A set of restrictions for domain object Skill
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public class SkillRestrictionSet : RestrictionSet<ISkill>, ISkillRestrictionSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillRestrictionSet"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public SkillRestrictionSet() : base(new List<IRestriction<ISkill>>{ new MinMaxAgents(), new MinMaxOccupancy() })
        {
        }

        private static ISkillRestrictionSet _currentRestrictionSet;

        /// <summary>
        /// Gets the current restriction set.
        /// </summary>
        /// <value>The current restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        public static ISkillRestrictionSet CurrentRestrictionSet
        {
            get
            {
                if (_currentRestrictionSet == null)
                    _currentRestrictionSet = new SkillRestrictionSet();
                return _currentRestrictionSet;
            }
        }
    }
}