using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
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

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[] { new PersonOrganizationData { PersonId = personId, SiteId = siteId } });

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

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[] { new PersonOrganizationData { PersonId = personId, TeamId = teamId } });

			var result = target.GetOrganization(personId);
			result.TeamId.Should().Be(teamId);
		}
	}
}