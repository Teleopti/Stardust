using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    ///  Interface for ApplicationFunctionRepository.
    /// </summary>
    public interface IApplicationFunctionRepository : IRepository<IApplicationFunction>
    {
        /// <summary>
        /// Gets all list order by name.
        /// </summary>
        /// <returns></returns>
        IList<IApplicationFunction> GetAllApplicationFunctionSortedByCode();

        IEnumerable<IApplicationFunction> ExternalApplicationFunctions();

	    IList<IApplicationFunction> GetChildFunctions(Guid id);
    }
}