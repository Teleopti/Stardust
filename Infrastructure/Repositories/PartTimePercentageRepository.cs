using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for part time percentages
    /// </summary>
    public class PartTimePercentageRepository : Repository<IPartTimePercentage>, IPartTimePercentageRepository
    {
        public PartTimePercentageRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public PartTimePercentageRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork, null, null)
	    {
		    
	    }

        /// <summary>
        /// Finds all part time percentage by description.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/31/2008
        /// </remarks>
        public ICollection<IPartTimePercentage>FindAllPartTimePercentageByDescription()
        {
            ICollection<IPartTimePercentage> retList = Session.CreateCriteria(typeof(PartTimePercentage))
                       .AddOrder(Order.Asc("Description"))
                       .List<IPartTimePercentage>();
            return retList;
        }
    }
}