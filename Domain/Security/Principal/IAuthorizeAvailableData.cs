using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IAuthorizeAvailableData
    {
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit);
    	bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization);
	    bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization);
	    bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization);
    }
}