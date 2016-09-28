using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

	    IEnumerable<IContract> FindContractsContain(string searchString, int maxHits);

		[Obsolete("Don't use! Shouldn't be here - use ICurrentUnitOfWork instead (or get the unitofwork in some other way).")]
		IUnitOfWork UnitOfWork { get; }
	}
}