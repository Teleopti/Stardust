using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for business units repository methods.
    /// </summary>
    public interface IBusinessUnitRepository : IRepository<IBusinessUnit>
    {
        /// <summary>
        /// Finds all business units sorted by name.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-05-22
        /// </remarks>
        IList<IBusinessUnit> LoadAllBusinessUnitSortedByName();

        /// <summary>
        /// Loads the hierarchy information.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit);

	    IList<Guid> LoadAllPersonsWithExternalLogOn(Guid businessUnitId, DateOnly now);
		/// <summary>
		/// Loads all time zones.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-08-11
		/// </remarks>
	    IEnumerable<TimeZoneInfo> LoadAllTimeZones();
    }
}