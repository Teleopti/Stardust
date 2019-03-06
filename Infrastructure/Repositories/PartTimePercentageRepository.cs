using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for part time percentages
    /// </summary>
    public class PartTimePercentageRepository : Repository<IPartTimePercentage>, IPartTimePercentageRepository
    {
		public static PartTimePercentageRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PartTimePercentageRepository(currentUnitOfWork, null, null);
		}

		public static PartTimePercentageRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new PartTimePercentageRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public PartTimePercentageRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
					: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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