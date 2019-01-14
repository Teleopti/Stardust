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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldMakeRegionalFromPerson()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("ar-SA"));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("ar-SA"));
			var target = new TeleoptiPrincipalInternalsFactory();

			var regional = target.MakeRegionalFromPerson(person);

			regional.Culture.Should().Be(CultureInfo.GetCultureInfo("ar-SA"));
			regional.UICulture.Should().Be(CultureInfo.GetCultureInfo("ar-SA"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldMakeRegionalWithoutCulture()
		{
			var person = PersonFactory.CreatePerson();
			var target = new TeleoptiPrincipalInternalsFactory();

			var regional = target.MakeRegionalFromPerson(person);

            Assert.That(regional.CultureLCID, Is.EqualTo(0));
            Assert.That(regional.UICultureLCID, Is.EqualTo(0));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldRetrieveThePersonsName()
		{
			var person = PersonFactory.CreatePerson();
			person.SetName(new Name("a", "person"));
			var target = new TeleoptiPrincipalInternalsFactory();

			var name = target.NameForPerson(person);

			name.Should().Be(person.Name.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldThrowPersonNotFoundExceptionIfThePersonIsNotFound()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			person.Stub(x => x.PrincipalName()).Throw(new NHibernate.ObjectNotFoundException(null, "Person"));
			var target = new TeleoptiPrincipalInternalsFactory();

			Assert.Throws<PersonNotFoundException>(() => target.NameForPerson(person));
		}
	}
}