using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for contracts
	/// </summary>
	public class ContractRepository : Repository<IContract>, IContractRepository
    {
        public ContractRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
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

		public IEnumerable<IContract> FindContractsContain(string searchString, int maxHits)
	    {
		    if (maxHits < 1)
			    return Enumerable.Empty<IContract>();

		    return Session.CreateCriteria<Contract>()
			    .Add(Restrictions.Like("Description.Name", searchString, MatchMode.Anywhere))
					.SetMaxResults(maxHits)
			    .List<IContract>();
	    }
    }
}