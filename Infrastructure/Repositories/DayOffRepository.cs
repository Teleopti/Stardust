using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for day off templates.
    /// </summary>
    /// <remarks>
    /// Created by: shirang
    /// Created date: 2008-10-28
    /// </remarks>
    public class DayOffRepository : Repository<IDayOffTemplate>, IDayOffRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        public DayOffRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

				public DayOffRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all contract by description.
        /// </summary>
        /// <returns>The list of <see cref="IDayOffTemplate"/>.</returns>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-28
        /// </remarks>
        public IList<IDayOffTemplate> FindAllDayOffsSortByDescription()
        {
            IList<IDayOffTemplate> retList = Session.CreateCriteria(typeof(IDayOffTemplate))
                       .AddOrder(Order.Asc("Description"))
                       .List<IDayOffTemplate>();
            return retList;
        }
    }
}