using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for part time percentages
    /// </summary>
    public class PartTimePercentageRepository : Repository<IPartTimePercentage>, IPartTimePercentageRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartTimePercentageRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public PartTimePercentageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PartTimePercentageRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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