using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for contracts
    /// </summary>
    public class ContractRepository : Repository<IContract>, IContractRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ContractRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

				public ContractRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all contract by description.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-14
        /// </remarks>
        public ICollection<IContract> FindAllContractByDescription()
        {
            ICollection<IContract> retList = Session.CreateCriteria(typeof (Contract))
                .SetFetchMode("MultiplicatorDefinitionSetCollection", FetchMode.Join)
                .AddOrder(Order.Asc("Description"))
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IContract>();

            return retList;
        }
    }
}