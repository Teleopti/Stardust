using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;
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
			var target = new TeamIdForPerson(personOrganizationReader);

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[] { new PersonOrganizationData{PersonId = personId, TeamId = teamId} });

			var result = target.GetTeamId(personId);
			result.Should().Be(teamId);
		}
	}

	public interface IPersonOrganizationReader
	{
		IEnumerable<PersonOrganizationData> LoadAll();
	}
	
	public class PersonOrganizationData
	{
		public Guid PersonId { get; set; }
		public Guid TeamId { get; set; }
	}

	public class TeamIdForPerson : ITeamIdForPerson
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public TeamIdForPerson(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public Guid GetTeamId(Guid personId)
		{
			var personData = _personOrganizationReader.LoadAll();
			return personData.Single(x => x.PersonId == personId).TeamId;
		}
	}
}