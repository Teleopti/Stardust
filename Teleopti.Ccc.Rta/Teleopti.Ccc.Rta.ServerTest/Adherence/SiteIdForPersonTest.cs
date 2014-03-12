using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class SiteIdForPersonTest
	{
		[Test]
		public void ShouldLoadSiteIdForPerson()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var personOrganizationReader = MockRepository.GenerateStub<IPersonOrganizationReader>();
			var target = new SiteIdForPerson(new PersonOrganizationProvider(personOrganizationReader));

			personOrganizationReader.Stub(x => x.LoadAll()).Return(new[] { new PersonOrganizationData{PersonId = personId, SiteId= siteId} });

			var result = target.GetSiteId(personId);
			result.Should().Be(siteId);
		}
	}
}