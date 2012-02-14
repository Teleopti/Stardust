using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    /// <summary>
    /// Interface for altering restrictions in RestrictionEditor and for providing data
    /// </summary>
    public interface IRestrictionAltered
    {
        /// <summary>
        /// Alters the restriction in Restrictioneditor
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-12-16
        /// </remarks>
        void RestrictionAltered();

        /// <summary>
        /// Gets a value indicating whether [restriction is altered].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [restriction is altered]; otherwise, <c>false</c>.
        /// </value>
        bool RestrictionIsAltered { get; set; }

        /// <summary>
        /// Removes the PersonRestrction
        /// </summary>
        /// <param name="restriction">The restriction.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-20
        /// </remarks>
        void RestrictionRemoved(IRestrictionViewModel restriction);

        /// <summary>
        /// Activities that are choosable to alter the target
        /// </summary>
        /// <value>The activities.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-16
        /// </remarks>
        IList<IActivity> Activities { get; }

        /// <summary>
        /// Gets the shift categories that are choosable to alter the target
        /// </summary>
        /// <value>The shift categories.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-16
        /// </remarks>
        IList<IShiftCategory> ShiftCategories { get; }

        /// <summary>
        /// Gets the day off templates that are choosable to alter the target
        /// </summary>
        /// <value>The day off templates.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-23
        /// </remarks>
        IList<IDayOffTemplate> DayOffTemplates { get; }
    }
}
