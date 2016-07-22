using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IBusinessUnitRepository : IRepository<IBusinessUnit>
    {
		IEnumerable<IBusinessUnit> LoadAllWithDeleted();

		IList<IBusinessUnit> LoadAllBusinessUnitSortedByName();

		IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit);

	    IEnumerable<TimeZoneInfo> LoadAllTimeZones();

    }
}