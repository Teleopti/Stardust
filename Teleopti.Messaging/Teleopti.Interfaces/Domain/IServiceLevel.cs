using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service Level class
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-06
    /// </remarks>
    public interface IServiceLevel : IEquatable<IServiceLevel>, 
                                    ICloneable
    {
        /// <summary>
        /// Gets or sets the percent.
        /// </summary>
        /// <value>The percent.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-06
        /// </remarks>
        Percent Percent { get; set; }

        /// <summary>
        /// Gets or sets the seconds.
        /// </summary>
        /// <value>The seconds.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-06
        /// </remarks>
        double Seconds { get; set; }
    }
}
