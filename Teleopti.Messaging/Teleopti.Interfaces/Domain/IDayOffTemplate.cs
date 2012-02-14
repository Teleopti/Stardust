#region Imports

using System;
using System.Drawing;

#endregion

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for DayOff.
    /// </summary>
    public interface IDayOffTemplate : IAggregateRoot, IChangeInfo
    {
        #region Properties - Instance Member

        /// <summary>
        /// Gets or sets the flexibility.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        TimeSpan Flexibility { get; }

        /// <summary>
        /// Gets or sets the length (duration).
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        TimeSpan TargetLength { get; }


        /// <summary>
        /// Sets the target and flexiblity.
        /// </summary>
        /// <param name="targetLength">Length of the target.</param>
        /// <param name="flexibility">The flexibility.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-04-17    
        /// /// </remarks>
        void SetTargetAndFlexibility(TimeSpan targetLength, TimeSpan flexibility);

        /// <summary>
        /// Gets or sets the anchor point relative to the start of the parent interval.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        TimeSpan Anchor { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets or sets the display color
        /// </summary>
        Color DisplayColor { get; set; }

    	///<summary>
		/// Gets or sets the payroll code
    	///</summary>
    	string PayrollCode	{ get; set; }

    	#endregion
    }
}