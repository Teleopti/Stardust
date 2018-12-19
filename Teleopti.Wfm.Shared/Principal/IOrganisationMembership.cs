using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IOrganisationMembership
    {
		bool BelongsToBusinessUnit(Guid businessUnitId, DateTime date);
		bool BelongsToSite(Guid siteId, DateTime date);
		bool BelongsToTeam(Guid teamId, DateTime date);
		bool IsUser(Guid? personId);
        IEnumerable<PeriodizedOrganisationMembership> Periods();
    }
}