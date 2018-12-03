using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class OrganisationMembershipExtensions
	{
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

	[Serializable]
	public class OrganisationMembership : IOrganisationMembership
	{
		private IEnumerable<PeriodizedOrganisationMembership> _periodizedOrganisationMembership = new List<PeriodizedOrganisationMembership>();
		private Guid _personId;

		public static IOrganisationMembership FromPerson(IPerson person)
		{
			var organization = new OrganisationMembership();
			organization.AddFromPerson(person);
			return organization;
		}

		public void AddFromPerson(IPerson person)
		{
			_personId = person.Id.GetValueOrDefault();
			_periodizedOrganisationMembership =
				person.PersonPeriodCollection.Where(pp => pp.Team != null && pp.Team.Site != null).Select(
					pp =>
					new PeriodizedOrganisationMembership(pp.StartDate, pp.EndDate(), pp.Team.Id.GetValueOrDefault(),
														 pp.Team.Site.Id.GetValueOrDefault(),
														 pp.Team.BusinessUnitExplicit?.Id.GetValueOrDefault() ?? Guid.Empty)).ToList();
		}
		
		public bool BelongsToBusinessUnit(Guid businessUnitId, DateOnly dateOnly)
		{
			var membership = findMembershipForDate(dateOnly);
			return membership.BelongsToBusinessUnit(businessUnitId);
		}

		private PeriodizedOrganisationMembership findMembershipForDate(DateOnly dateOnly)
		{
			foreach (var periodizedOrganisationMembership in _periodizedOrganisationMembership)
			{
				if (periodizedOrganisationMembership.BelongsToDate(dateOnly))
					return periodizedOrganisationMembership;
			}
			return new PeriodizedOrganisationMembership(DateOnly.MinValue,DateOnly.MinValue,Guid.Empty,Guid.Empty,Guid.Empty);
		}
		
		public bool BelongsToSite(Guid siteId, DateOnly dateOnly)
		{
			var membership = findMembershipForDate(dateOnly);
			return membership.BelongsToSite(siteId);
		}

		public bool BelongsToTeam(Guid teamId, DateOnly dateOnly)
		{
			var membership = findMembershipForDate(dateOnly);
			return membership.BelongsToTeam(teamId);
		}

		public bool IsUser(Guid? personId)
		{
			return _personId == personId.GetValueOrDefault();
		}

		public IEnumerable<DateOnlyPeriod> Periods()
		{
			return _periodizedOrganisationMembership.Select(p => p.Period);
		}

		[Serializable]
		private class PeriodizedOrganisationMembership
		{
			private readonly Guid _teamId;
			private readonly Guid _siteId;
			private readonly Guid _businessUnitId;
			private DateOnlyPeriod _period;

			public PeriodizedOrganisationMembership(DateOnly startDate, DateOnly endDate, Guid teamId, Guid siteId, Guid businessUnitId)
			{
				_teamId = teamId;
				_siteId = siteId;
				_businessUnitId = businessUnitId;
				_period = new DateOnlyPeriod(startDate, endDate);
			}

			public DateOnlyPeriod Period => _period;

			public bool BelongsToDate(DateOnly dateOnly)
			{
				return _period.Contains(dateOnly);
			}

			public bool BelongsToTeam(Guid teamId)
			{
				return _teamId.Equals(teamId);
			}

			public bool BelongsToSite(Guid siteId)
			{
				return _siteId.Equals(siteId);
			}

			public bool BelongsToBusinessUnit(Guid businessUnitId)
			{
				return _businessUnitId.Equals(businessUnitId);
			}
		}
	}
}