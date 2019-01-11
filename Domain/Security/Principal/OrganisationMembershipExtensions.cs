using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class OrganisationMembershipExtensions
	{
		public static IOrganisationMembership InitializeFromPerson(this OrganisationMembership instance, IPerson person)
		{
			if (person == null)
				return instance;
			instance.PersonId = person.Id.GetValueOrDefault();
			instance.Membership =
				person.PersonPeriodCollection.Where(pp => pp.Team != null && pp.Team.Site != null).Select(
					pp =>
						new PeriodizedOrganisationMembership(pp.StartDate, pp.EndDate(), pp.Team.Id.GetValueOrDefault(),
							pp.Team.Site.Id.GetValueOrDefault(),
							pp.Team.BusinessUnitExplicit?.Id.GetValueOrDefault() ?? Guid.Empty)).ToList();
			return instance;
		}

		public static bool BelongsToBusinessUnit(this IOrganisationMembership instance, IBusinessUnit businessUnit, DateOnly dateOnly)
		{
			return instance.BelongsToBusinessUnit(businessUnit.Id.GetValueOrDefault(), dateOnly);
		}

		public static bool BelongsToSite(this IOrganisationMembership instance, ISite site, DateOnly dateOnly)
		{
			return instance.BelongsToSite(site.Id.GetValueOrDefault(), dateOnly);
		}

		public static bool BelongsToTeam(this IOrganisationMembership instance, ITeam team, DateOnly dateOnly)
		{
			return instance.BelongsToTeam(team.Id.GetValueOrDefault(), dateOnly);
		}

		public static bool IsUser(this IOrganisationMembership instance, IPerson person)
		{
			return person != null && instance.IsUser(person.Id);
		}
	}
}