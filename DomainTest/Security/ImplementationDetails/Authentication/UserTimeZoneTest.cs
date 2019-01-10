using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Authentication
{
	[TestFixture]
	public class UserTimeZoneTest
	{
		[Test]
		public void ShouldGetTimeZoneFromLoggedOnUser()
		{
			var currentPrincipal = new FakeCurrentTeleoptiPrincipal();
			var user = new Person();
			user.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
			currentPrincipal.Fake(new TeleoptiPrincipal(new TeleoptiIdentity("user",null,null,null, null), user));
			var target = new UserTimeZone(currentPrincipal);

			var result = target.TimeZone();

			result.Should().Be.EqualTo(user.PermissionInformation.DefaultTimeZone());
		}

		[Test]
		public void ShouldUseCachedPersonTimezoneWhenApplicableToGetTheLatestTimezoneUpdateFromFatClient()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.AustralianTimeZoneInfo());
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			principal.Regional = new Regional(TimeZoneInfoFactory.ChinaTimeZoneInfo(), CultureInfo.CurrentCulture, CultureInfo.CurrentCulture);

			var currentPrincipal = new FakeCurrentTeleoptiPrincipal(principal);
			var target = new UserTimeZone(currentPrincipal);
			target.TimeZone().Should().Be(TimeZoneInfoFactory.AustralianTimeZoneInfo());
		}


		[Test]
		public void ShouldReturnNullIfNotLoggedOn()
		{
			var target = new UserTimeZone(new FakeCurrentTeleoptiPrincipal());
			target.TimeZone().Should().Be.Null();
		}
	}
}