using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for loading ACD logins
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-18
    /// </remarks>
    public interface IExternalLogOnRepository : IRepository<IExternalLogOn>
    {
        /// <summary>
        /// Loads all ACD logins.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        IList<IExternalLogOn> LoadAllExternalLogOns();
    }
}