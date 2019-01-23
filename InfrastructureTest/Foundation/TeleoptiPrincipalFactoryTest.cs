using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using PersonAndBusinessUnit = Teleopti.Ccc.Domain.Logon.PersonAndBusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class TeleoptiPrincipalFactoryTest
	{
		[Test]
		public void ShouldMakeRegionalFromPerson()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.BrazilTimeZoneInfo());

			var internalsFactory = new TeleoptiPrincipalInternalsFactory();
			var target = new TeleoptiPrincipalFactory(internalsFactory);

			var principal = target.MakePrincipal(new PersonAndBusinessUnit(person, BusinessUnitFactory.CreateSimpleBusinessUnit("bu")), new FakeDataSource("fake"), "token");

			principal.Regional.TimeZone
				.Should()
				.Be.EqualTo(TimeZoneInfoFactory.BrazilTimeZoneInfo());
		}

	}
}
