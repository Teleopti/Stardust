using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class OrganisationMembership : IOrganisationMembership
	{
		public IEnumerable<PeriodizedOrganisationMembership> Membership { get; set; } = Enumerable.Empty<PeriodizedOrganisationMembership>();
		public Guid PersonId { get; set; }

		public bool BelongsToBusinessUnit(Guid businessUnitId, DateOnly dateOnly) => membershipForDate(dateOnly).BelongsToBusinessUnit(businessUnitId);
		public bool BelongsToSite(Guid siteId, DateOnly dateOnly) => membershipForDate(dateOnly).BelongsToSite(siteId);
		public bool BelongsToTeam(Guid teamId, DateOnly dateOnly) => membershipForDate(dateOnly).BelongsToTeam(teamId);
		public bool IsUser(Guid? personId) => PersonId == personId.GetValueOrDefault();
		public IEnumerable<DateOnlyPeriod> Periods() => Membership.Select(p => p.Period);

		private PeriodizedOrganisationMembership membershipForDate(DateOnly date)
		{
			var membership = Membership.FirstOrDefault(x => x.BelongsToDate(date));
			return membership ?? new PeriodizedOrganisationMembership(DateOnly.MinValue, DateOnly.MinValue, Guid.Empty, Guid.Empty, Guid.Empty);
		}
	}

	[Serializable]
	public class PeriodizedOrganisationMembership
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
		public bool BelongsToDate(DateOnly dateOnly) => _period.Contains(dateOnly);
		public bool BelongsToTeam(Guid teamId) => _teamId.Equals(teamId);
		public bool BelongsToSite(Guid siteId) => _siteId.Equals(siteId);
		public bool BelongsToBusinessUnit(Guid businessUnitId) => _businessUnitId.Equals(businessUnitId);
	}
}