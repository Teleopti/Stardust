using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests.Adherence
{
	public class OrganizationForPersonTest
	{
		[Test]
		public void ShouldLoadSiteIdForPerson()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var personOrganizationReader = MockRepository.GenerateStub<IPersonOrganizationReader>();
			var target = new OrganizationForPerson(new PersonOrganizationProvider(personOrganizationReader));

			personOrganizationReader.Stub(x => x.PersonOrganizationData()).Return(new[] { new PersonOrganizationData { PersonId = personId, SiteId = siteId } });

			var result = target.GetOrganization(personId);
			result.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldLoadTeamIdForPerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var personOrganizationReader = MockRepository.GenerateStub<IPersonOrganizationReader>();
			var target = new OrganizationForPerson(new PersonOrganizationProvider(personOrganizationReader));

			personOrganizationReader.Stub(x => x.PersonOrganizationData()).Return(new[] { new PersonOrganizationData { PersonId = personId, TeamId = teamId } });

			var result = target.GetOrganization(personId);
			result.TeamId.Should().Be(teamId);
		}
	}
}