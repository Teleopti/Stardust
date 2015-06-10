using System;
using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class TenantAuthenticationTest
	{
		[Test]
		public void ShouldSetPersonInfoIfSuccessful()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var expected = new PersonInfo();
			var httpcontext = createHttpContext(personId, tenantPassword);

			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(expected);

			var target = new TenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials);
			target.Logon()
				.Should().Be.True();
			httpcontext.Items[TenantAuthentication.PersonInfoKey].Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldNotSetPersonInfoIfUnsuccessful()
		{
			var personId = Guid.NewGuid();
			var tenantPassword = RandomName.Make();
			var httpcontext = createHttpContext(personId, tenantPassword);

			var findPersonByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonByCredentials.Expect(x => x.Find(personId, tenantPassword)).Return(null);

			var target = new TenantAuthentication(new FakeCurrentHttpContext(httpcontext), findPersonByCredentials);
			target.Logon()
				.Should().Be.False();
			httpcontext.Items[TenantAuthentication.PersonInfoKey].Should().Be.Null();
		}

		private static HttpContextBase createHttpContext(Guid personId, string tenantPassword)
		{
			var headers = new NameValueCollection {{"personid", personId.ToString()}, {"tenantpassword", tenantPassword}};
			return new FakeHttpContext(string.Empty, string.Empty, null, null, null, null, null, headers);
		}
	}
}