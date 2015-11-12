using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Repository for contracts
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-22
    /// </remarks>
    public interface IContractRepository : IRepository<IContract>
    {
        /// <summary>
        /// Finds all contract by description.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-14
        /// </remarks>
        ICollection<IContract> FindAllContractByDescription();

	    IEnumerable<IContract> FindContractsStartWith(string searchString);
    }
}