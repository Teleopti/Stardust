using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Ccc.AgentPortal.Common
{
    public interface IHelpContext
    {
        /// <summary>
        /// Gets a value indicating whether this instance has help information.
        /// Default is True
        /// </summary>
        /// <value><c>true</c> if this instance has help information; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-01
        /// </remarks>
        bool HasHelp { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-12
        /// </remarks>
        string Name { get; }
    }
}
