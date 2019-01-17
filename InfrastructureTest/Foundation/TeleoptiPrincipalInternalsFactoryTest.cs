using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class TeleoptiPrincipalInternalsFactoryTest
	{
		[Test]
		public void ShouldRetrieveThePersonsName()
		{
			var person = PersonFactory.CreatePerson();
			person.SetName(new Name("a", "person"));
			var target = new TeleoptiPrincipalInternalsFactory();

			var name = target.NameForPerson(person);

			name.Should().Be(person.Name.ToString());
		}

		[Test]
		public void ShouldThrowPersonNotFoundExceptionIfThePersonIsNotFound()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			person.Stub(x => x.PrincipalName()).Throw(new NHibernate.ObjectNotFoundException(null, "Person"));
			var target = new TeleoptiPrincipalInternalsFactory();

			Assert.Throws<PersonNotFoundException>(() => target.NameForPerson(person));
		}
	}
}