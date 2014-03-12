using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class PersonOrganizationReaderTest
	{
		[Test]
		public void ShouldOnlyLoadOnceForTeam()
		{
			var personOrganizationReader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			var target = new TeamIdForPerson(new PersonOrganizationProvider(personOrganizationReader));

			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[]
				{
					new PersonOrganizationData {PersonId = personId1},
					new PersonOrganizationData {PersonId = personId2}
				});

			target.GetTeamId(personId1);
			target.GetTeamId(personId2);

			personOrganizationReader.AssertWasCalled(x => x.LoadAll(), a => a.Repeat.Once());
		}

		[Test]
		public void ShouldOnlyLoadOnceForSite()
		{
			var personOrganizationReader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			var target = new SiteIdForPerson(new PersonOrganizationProvider(personOrganizationReader));

			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[]
				{
					new PersonOrganizationData {PersonId = personId1},
					new PersonOrganizationData {PersonId = personId2}
				});

			target.GetSiteId(personId1);
			target.GetSiteId(personId2);

			personOrganizationReader.AssertWasCalled(x => x.LoadAll(), a => a.Repeat.Once());
		}

		[Test]
		public void ShouldOnlyLoadOnceForSiteAndTeam()
		{
			var personOrganizationReader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			var provider = new PersonOrganizationProvider(personOrganizationReader);
			var siteIdProvider = new SiteIdForPerson(provider);
			var teamIdProvider = new TeamIdForPerson(provider);

			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[]
				{
					new PersonOrganizationData {PersonId = personId1},
					new PersonOrganizationData {PersonId = personId2}
				});

			siteIdProvider.GetSiteId(personId1);
			teamIdProvider.GetTeamId(personId2);

			personOrganizationReader.AssertWasCalled(x => x.LoadAll(), a => a.Repeat.Once());
		}
	}

}