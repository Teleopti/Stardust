using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IOrganisationMembership
    {
		bool BelongsToBusinessUnit(Guid businessUnitId, DateOnly dateOnly);
		bool BelongsToSite(Guid siteId, DateOnly dateOnly);
		bool BelongsToTeam(Guid teamId, DateOnly dateOnly);
		bool IsUser(Guid? personId);
        IEnumerable<DateOnlyPeriod> Periods();
    }
}