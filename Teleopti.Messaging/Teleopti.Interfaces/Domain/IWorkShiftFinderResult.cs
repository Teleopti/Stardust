using System;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds the result of a try to find a workshift on a Person and Date
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-23    
    /// /// </remarks>
    public interface IWorkShiftFinderResult
    {
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        IPerson Person { get; }
        /// <summary>
        /// Gets the person date key.
        /// </summary>
        /// <value>The person date key.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-15
        /// </remarks>
        Tuple<Guid,DateOnly> PersonDateKey { get; }

        /// <summary>
        /// Gets the name of the person.
        /// </summary>
        /// <value>The name of the person.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-20    
        /// /// </remarks>
        string PersonName { get; }
        /// <summary>
        /// Gets the schedule date.
        /// </summary>
        /// <value>The schedule date.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        DateOnly ScheduleDate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IWorkShiftFinderResult"/> is successful.
        /// </summary>
        /// <value><c>true</c> if successful; otherwise, <c>false</c>.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        bool Successful { get; set; }

        /// <summary>
        /// Gets or sets the filter results.
        /// </summary>
        /// <value>The filter results.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
		ReadOnlyCollection<IWorkShiftFilterResult> FilterResults { get; }


        /// <summary>
        /// Gets or sets the date and time for doing the scheduling.
        /// </summary>
        /// <value>The scheduling date time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-20    
        /// /// </remarks>
        DateTime SchedulingDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [stopped on overstaffing].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [stopped on overstaffing]; otherwise, <c>false</c>.
        /// </value>
        bool StoppedOnOverstaffing { get; set; }

		/// <summary>
		/// Adds the filter results.
		/// </summary>
		/// <param name="filterResult">The filter result.</param>
    	void AddFilterResults(IWorkShiftFilterResult filterResult);
    }
}
