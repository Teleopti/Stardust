using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class CurrentTenantUserTest
	{
		[Test]
		public void ShouldReturnNullIfNotExists()
		{
			new CurrentTenantUser(new FakeCurrentHttpContext(new FakeHttpContext())).CurrentUser()
				.Should().Be.Null();
		}

		[Test]
		public void ShouldGetPersonInfo()
		{
			var expected = new PersonInfo();
			var httpContext = new FakeHttpContext();
			httpContext.Items[TenantAuthentication.PersonInfoKey] = expected;

			new CurrentTenantUser(new FakeCurrentHttpContext(httpContext)).CurrentUser()
					.Should().Be.SameInstanceAs(expected);
		}
	}
}