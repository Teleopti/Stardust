using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPartTimePercentageRepository : IRepository<IPartTimePercentage>
    {
        /// <summary>
        /// Finds all part time percentage by description.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 3/31/2008
        /// </remarks>
        ICollection<IPartTimePercentage>FindAllPartTimePercentageByDescription();
    }
}