using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[Serializable]
	public class OrganisationMembership : IOrganisationMembership
	{
		public IEnumerable<PeriodizedOrganisationMembership> Membership { get; set; } = Enumerable.Empty<PeriodizedOrganisationMembership>();
		public Guid PersonId { get; set; }

		public bool BelongsToBusinessUnit(Guid businessUnitId, DateTime date) => membershipForDate(date).BelongsToBusinessUnit(businessUnitId);
		public bool BelongsToSite(Guid siteId, DateTime date) => membershipForDate(date).BelongsToSite(siteId);
		public bool BelongsToTeam(Guid teamId, DateTime date) => membershipForDate(date).BelongsToTeam(teamId);
		public bool IsUser(Guid? personId) => PersonId == personId.GetValueOrDefault();
		public IEnumerable<PeriodizedOrganisationMembership> Periods() => Membership;

		private PeriodizedOrganisationMembership membershipForDate(DateTime date)
		{
			var membership = Membership.FirstOrDefault(x => x.BelongsToDate(date));
			return membership ?? new PeriodizedOrganisationMembership(DateTime.MinValue, DateTime.MinValue, Guid.Empty, Guid.Empty, Guid.Empty);
		}
	}

	[Serializable]
	public class PeriodizedOrganisationMembership
	{
		private readonly Guid _teamId;
		private readonly Guid _siteId;
		private readonly Guid _businessUnitId;

		public PeriodizedOrganisationMembership(DateTime startDate, DateTime endDate, Guid teamId, Guid siteId, Guid businessUnitId)
		{
			StartDate = startDate;
			EndDate = endDate;
			_teamId = teamId;
			_siteId = siteId;
			_businessUnitId = businessUnitId;
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public bool BelongsToDate(DateTime date) => date >= StartDate && date <= EndDate;
		public bool BelongsToTeam(Guid teamId) => _teamId.Equals(teamId);
		public bool BelongsToSite(Guid siteId) => _siteId.Equals(siteId);
		public bool BelongsToBusinessUnit(Guid businessUnitId) => _businessUnitId.Equals(businessUnitId);
	}
}