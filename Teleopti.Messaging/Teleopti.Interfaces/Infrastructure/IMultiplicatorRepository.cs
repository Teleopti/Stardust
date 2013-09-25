#region Imports

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Interfaces.Infrastructure
{

    /// <summary>
    /// Defines the functionality of IMultiplicatorRepository.
    /// </summary>
    public interface IMultiplicatorRepository : IRepository<IMultiplicator>
    {

        #region Properties - Instance Member

        #endregion

        #region Methods - Instance Member

        /// <summary>
        /// Loads the name of all sort by.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-08
        /// </remarks>
        IList<IMultiplicator> LoadAllSortByName();

        /// <summary>
        /// Loads the name of all by type and sort by.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IList<IMultiplicator> LoadAllByTypeAndSortByName(MultiplicatorType type);

        #endregion

    }

}
