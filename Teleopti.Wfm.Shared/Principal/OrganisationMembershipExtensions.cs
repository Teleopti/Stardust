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
				(person.PrincipalPeriods() ?? Enumerable.Empty<IPrincipalSourcePeriod>())
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
	}
}