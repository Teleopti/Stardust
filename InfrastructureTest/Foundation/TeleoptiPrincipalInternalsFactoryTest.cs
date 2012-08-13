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
	public class TeleoptiPrincipalInternalsFactoryTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldRetrieveThePersonsName()
		{
			var person = PersonFactory.CreatePerson();
			person.Name = new Name("a", "person");
			var target = new TeleoptiPrincipalInternalsFactory();
			var name = target.NameForPerson(person);
			name.Should().Be(person.Name.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldThrowPersonNotFoundExceptionIfThePersonIsNotFound()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			person.Stub(x => x.Name).Throw(new NHibernate.ObjectNotFoundException(null, "Person"));
			var target = new TeleoptiPrincipalInternalsFactory();
			Assert.Throws<PersonNotFoundException>(() => target.NameForPerson(person));
		}
	}
}