using System;
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
        })
        {
        }

        private static readonly Lazy<PersonAssignmentRestrictionSet> _personAssignmentRestrictionSet = new Lazy<PersonAssignmentRestrictionSet>(()=>new PersonAssignmentRestrictionSet());

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
            get { return _personAssignmentRestrictionSet.Value; }
        }
    }
}