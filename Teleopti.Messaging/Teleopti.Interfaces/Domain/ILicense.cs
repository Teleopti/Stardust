using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// contains the license. There can be only one!
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-11-18
    /// </remarks>
    public interface ILicense : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the XML string containing the license
        /// </summary>
        /// <value>The XML string.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-18
        /// </remarks>
        string XmlString { get; set; }
    }
}
