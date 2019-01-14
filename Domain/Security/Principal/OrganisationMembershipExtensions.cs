using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class OrganisationMembershipExtensions
	{
		public static IOrganisationMembership Initialize(this OrganisationMembership instance, IPrincipalSource person)
		{
			if (person == null)
				return instance;
			instance.PersonId = person.PrincipalPersonId();
			instance.Membership =
				person.PrincipalPeriods()
					.Where(x => x.PrincipalTeamId() != null && x.PrincipalSiteId() != null)
					.Select(x =>
						new PeriodizedOrganisationMembership(
							x.PrincipalStartDate(),
							x.PrincipalEndDate(),
							x.PrincipalTeamId().GetValueOrDefault(),
							x.PrincipalSiteId().GetValueOrDefault(),
							x.PrincipalBusinessUnitId().GetValueOrDefault())
					).ToList();
			return instance;
		}

		public static bool BelongsToBusinessUnit(this IOrganisationMembership instance, IBusinessUnit businessUnit, DateOnly dateOnly) =>
			instance.BelongsToBusinessUnit(businessUnit.Id.GetValueOrDefault(), dateOnly.Date);

		public static bool BelongsToBusinessUnit(this IOrganisationMembership instance, Guid businessUnit, DateOnly dateOnly) =>
			instance.BelongsToBusinessUnit(businessUnit, dateOnly.Date);

		public static bool BelongsToSite(this IOrganisationMembership instance, ISite site, DateOnly dateOnly) =>
			instance.BelongsToSite(site.Id.GetValueOrDefault(), dateOnly.Date);

		public static bool BelongsToSite(this IOrganisationMembership instance, Guid site, DateOnly dateOnly) =>
			instance.BelongsToSite(site, dateOnly.Date);

		public static bool BelongsToTeam(this IOrganisationMembership instance, ITeam team, DateOnly dateOnly) =>
			instance.BelongsToTeam(team.Id.GetValueOrDefault(), dateOnly.Date);

		public static bool BelongsToTeam(this IOrganisationMembership instance, Guid team, DateOnly dateOnly) =>
			instance.BelongsToTeam(team, dateOnly.Date);

		public static bool IsUser(this IOrganisationMembership instance, IPerson person) =>
			person != null && instance.IsUser(person.Id);
	}
}