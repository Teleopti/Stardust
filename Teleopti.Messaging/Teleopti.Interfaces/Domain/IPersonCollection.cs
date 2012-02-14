using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Collection wrapping persons and exposing permitted organization
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 2009-06-09
    /// </remarks>
    public interface IPersonCollection
    {
        /// <summary>
        /// Gets all permitted persons.
        /// </summary>
        /// <value>All permitted persons.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-06-09
        /// </remarks>
        IEnumerable<IPerson> AllPermittedPersons { get; }
    }
}
