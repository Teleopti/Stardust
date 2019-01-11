﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class TeleoptiPrincipalCacheableFactoryTest
	{
		[Test]
		public void ShouldMakeRegionalFromPerson()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.BrazilTimeZoneInfo());

			var internalsFactory = new TeleoptiPrincipalInternalsFactory();
			var target = new TeleoptiPrincipalFactory(internalsFactory, internalsFactory, internalsFactory);

			var principal = target.MakePrincipal(person, new FakeDataSource("fake"), BusinessUnitFactory.CreateSimpleBusinessUnit("bu"), "token");

//			(principal as TeleoptiPrincipalCacheable).Person.PermissionInformation.DefaultTimeZone()
			principal.Regional.TimeZone
				.Should()
				.Be.EqualTo(TimeZoneInfoFactory.BrazilTimeZoneInfo());
		}

	}
}
