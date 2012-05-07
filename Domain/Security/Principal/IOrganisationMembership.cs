using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IOrganisationMembership
    {
        bool BelongsToBusinessUnit(IBusinessUnit businessUnit, DateOnly dateOnly);
        bool BelongsToSite(ISite site, DateOnly dateOnly);
        bool BelongsToTeam(ITeam team, DateOnly dateOnly);
        bool IsUser(IPerson person);
        IEnumerable<DateOnlyPeriod> Periods();
    }
}