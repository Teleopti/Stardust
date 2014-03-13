using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class TeamIdForPersonTest
	{
		[Test]
		public void ShouldLoadTeamIdForPerson()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var personOrganizationReader = MockRepository.GenerateStub<IPersonOrganizationReader>();
			var target = new TeamIdForPerson(new PersonOrganizationProvider(personOrganizationReader));

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[] { new PersonOrganizationData{PersonId = personId, TeamId = teamId} });

			var result = target.GetTeamId(personId);
			result.Should().Be(teamId);
		}
	}
}