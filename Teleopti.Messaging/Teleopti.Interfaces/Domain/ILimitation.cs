using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for TimeLimitation
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-12-10
    /// </remarks>
    public interface ILimitation
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
		TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Sets the start time string.
        /// </summary>
        /// <value>The start time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        string StartTimeString { set; get; }

        /// <summary>
        /// Gets or sets the end time string.
        /// </summary>
        /// <value>The end time string.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-15    
        /// /// </remarks>
        string EndTimeString { set; get; }

        /// <summary>
        /// Exposes the parselogic for parsing string to timespan
        /// </summary>
        /// <param name="value">The time string.</param>
        /// <returns></returns>
        /// <remarks>
        /// Exposed for handling Validation in the presentation-layer
        /// Created by: henrika
        /// Created date: 2009-01-26
        /// </remarks>
		TimeSpan? TimeSpanFromString(string value);

        /// <summary>
        /// Exposes the parselogic for converting TimeSpan to string
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Exposed for handling Validation in the presentation-layer
        /// Created by: henrika
        /// Created date: 2009-01-27
        /// </remarks>
        string StringFromTimeSpan(TimeSpan? value);

        /// <summary>
        /// Determines whether this instance has value.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if StartTime or EndTime has value; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-28
        /// </remarks>
        bool HasValue();

		 /// <summary>
		 /// Determines if limitation is valid for <paramref name="timeSpan"/>
		 /// </summary>
    	bool IsValidFor(TimeSpan timeSpan);
    }
}
