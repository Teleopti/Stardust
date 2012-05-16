using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class TeleoptiPrincipalCacheableFactoryTest
	{
		[Test]
		public void ShouldMakeCachablePrincipal()
		{
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var internalsFactory = new TeleoptiPrincipalInternalsFactory();
			var target = new TeleoptiPrincipalCacheableFactory(internalsFactory, internalsFactory, internalsFactory);

			var result = target.MakePrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Application);

			result.Should().Be.OfType<TeleoptiPrincipalCacheable>();
		}

		[Test]
		public void ShouldMakeUsingCachableDependecies()
		{
			var person = PersonFactory.CreatePerson();
			var makeRegionalFromPerson = MockRepository.GenerateMock<IMakeRegionalFromPerson>();
			var makeOrganizationFromPerson = MockRepository.GenerateMock<IMakeOrganisationMembershipFromPerson>();
			var retrievePersonNameForPerson = MockRepository.GenerateMock<IRetrievePersonNameForPerson>();
			var target = new TeleoptiPrincipalCacheableFactory(makeRegionalFromPerson, makeOrganizationFromPerson, retrievePersonNameForPerson);
			retrievePersonNameForPerson.Stub(x => x.NameForPerson(person)).Return(person.Name.ToString());

			target.MakePrincipal(person, null, null, AuthenticationTypeOption.Windows);

			makeRegionalFromPerson.AssertWasCalled(x => x.MakeRegionalFromPerson(person));
			makeOrganizationFromPerson.AssertWasCalled(x => x.MakeOrganisationMembership(person));
			retrievePersonNameForPerson.AssertWasCalled(x => x.NameForPerson(person));
		}
	}
}