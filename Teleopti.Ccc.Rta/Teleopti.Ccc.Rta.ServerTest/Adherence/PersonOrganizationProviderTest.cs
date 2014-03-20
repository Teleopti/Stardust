using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class PersonOrganizationProviderTest
	{
		[Test]
		public void ShouldOnlyLoadOnce()
		{
			var personOrganizationReader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			var target = new PersonOrganizationProvider(personOrganizationReader);

			personOrganizationReader.Expect(x => x.LoadAll()).Return(Enumerable.Empty<PersonOrganizationData>()).Repeat.Once();

			target.LoadAll();
			target.LoadAll();
		}
		
		[Test]
		public void ShouldOnlyLoadOnceForSiteAndTeam()
		{
			var personOrganizationReader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			var provider = new PersonOrganizationProvider(personOrganizationReader);
			var siteIdProvider = new OrganizationForPerson(provider);
			var teamIdProvider = new OrganizationForPerson(provider);

			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[]
				{
					new PersonOrganizationData {PersonId = personId1},
					new PersonOrganizationData {PersonId = personId2}
				});

			siteIdProvider.GetOrganization(personId1);
			teamIdProvider.GetOrganization(personId2);

			personOrganizationReader.AssertWasCalled(x => x.LoadAll(), a => a.Repeat.Once());
		}
	}

}