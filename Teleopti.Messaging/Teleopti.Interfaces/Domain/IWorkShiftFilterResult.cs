using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for showing result when finding the Best Shift
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-23    
    /// /// </remarks>
    public interface IWorkShiftFilterResult
    {
        
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        string Message { get;  }

        /// <summary>
        /// Gets number of work shifts before the filter.
        /// </summary>
        /// <value>The work shifts before.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        int WorkShiftsBefore { get; }

        /// <summary>
        /// Gets number of work shifts after the filter.
        /// </summary>
        /// <value>The work shifts after.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-23    
        /// /// </remarks>
        int WorkShiftsAfter { get; }

		/// <summary>
		/// Gets the key used if we want to make shure we only get one item in the list.
		/// </summary>
		/// <value>The key.</value>
    	Guid Key { get; }
    }
}