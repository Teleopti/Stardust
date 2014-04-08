using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Interface for <see cref="LicenseActivator" /> class.
	/// </summary>
    public interface ILicenseActivator
    {
        /// <summary>
        /// Gets or sets the name of the enabled license schema.
        /// </summary>
        /// <value>The name of the enabled license schema.</value>
        string EnabledLicenseSchemaName { get; }

        /// <summary>
        /// Gets the name of the licensed customer.
        /// </summary>
        /// <value>The name of the customer.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-27
        /// </remarks>
        string CustomerName { get; }

        /// <summary>
        /// Gets the expiration date for the license.
        /// </summary>
        /// <value>The expiration date.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-27
        /// </remarks>
        DateTime ExpirationDate { get; }

        /// <summary>
        /// Gets the max active agents licensed.
        /// </summary>
        /// <value>The max active agents.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-27
        /// </remarks>
        int MaxActiveAgents { get; }

        /// <summary>
        /// Gets the max active agents grace percent that MaxActiveAgents can be exceeded by until it is impossible to add more.
        /// </summary>
        /// <value>The max active agents grace.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-27
        /// </remarks>
        Percent MaxActiveAgentsGrace { get; }


        /// <summary>
        /// Determines whether the specified number of active agents is this too many for the license.
        /// </summary>
        /// <param name="activeAgents">The active agents.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-02
        /// </remarks>
        bool IsThisTooManyActiveAgents(int activeAgents);

        /// <summary>
        /// Determines whether the specified number of active agents is too many for the license, but the grace amount is not necessarily exceeded yet
        /// </summary>
        /// <param name="activeAgents">The active agents.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-02
        /// </remarks>
        bool IsThisAlmostTooManyActiveAgents(int activeAgents);
        
        /// <summary>
        /// Gets or sets the enabled license option paths.
        /// </summary>
        /// <value>The enabled license option paths.</value>
        IList<string> EnabledLicenseOptionPaths { get; }

		int MaxSeats { get; }

		LicenseType LicenseType { get; }
    }
}