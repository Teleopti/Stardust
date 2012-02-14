using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// A set of restrictions for domain object PersonAssignment
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public class PersonAssignmentRestrictionSet : RestrictionSet<IPersonAssignment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAssignmentRestrictionSet"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public PersonAssignmentRestrictionSet()
            : base(new List<IRestriction<IPersonAssignment>>
        {
            new AssignmentLength(), 
            new GapsInAssignment()
        })
        {
        }

        private static PersonAssignmentRestrictionSet _personAssignmentRestrictionSet;

        /// <summary>
        /// Gets the current person assignment restriction set.
        /// </summary>
        /// <value>The current person assignment restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        public static PersonAssignmentRestrictionSet CurrentPersonAssignmentRestrictionSet
        {
            get
            {
                if (_personAssignmentRestrictionSet == null)
                    _personAssignmentRestrictionSet = new PersonAssignmentRestrictionSet();
                return _personAssignmentRestrictionSet;
            }
        }
    }
}