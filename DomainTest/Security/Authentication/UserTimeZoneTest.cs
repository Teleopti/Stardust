using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
	[TestFixture]
	public class UserTimeZoneTest
	{
		[Test]
		public void ShouldGetTimeZoneFromLoggedOnUser()
		{
			var user = new Person();
			user.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			var target = new UserTimeZone(loggedOnUser);

			var result = target.TimeZone();

			result.Should().Be.EqualTo(user.PermissionInformation.DefaultTimeZone());
		}

		[Test]
		public void ShouldReturnNullIfNotLoggedOn()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(null);
			var target = new UserTimeZone(loggedOnUser);
			target.TimeZone().Should().Be.Null();
		}
	}
}