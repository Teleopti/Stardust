using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class IAuthorizeAvailableDataExtensions
	{
		public static bool Check(this IAuthorizeAvailableData instance, IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
		{
			return instance.Check(queryingPerson, dateOnly, businessUnit?.Id.GetValueOrDefault());
		}
		
		public static bool Check(this IAuthorizeAvailableData instance, IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid? businessUnitId)
		{
			return instance.Check(queryingPerson, dateOnly, businessUnitId.GetValueOrDefault());
		}
	}
	
    public interface IAuthorizeAvailableData
    {
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site);
        bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId);
    	bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization);
	    bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization);
	    bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization);
    }
}