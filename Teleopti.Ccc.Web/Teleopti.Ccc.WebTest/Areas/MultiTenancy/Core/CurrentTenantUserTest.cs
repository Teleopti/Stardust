using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class CurrentTenantUserTest
	{
		[Test]
		public void ShouldReturnNullIfNotExist()
		{
			var sessionDataProvider = MockRepository.GenerateStub<ISessionSpecificDataProvider>();
			sessionDataProvider.Expect(x => x.GrabFromCookie()).Return(null);
			new CurrentTenantUser(new FakeCurrentHttpContext(new FakeHttpContext()), sessionDataProvider, null).CurrentUser()
				.Should().Be.Null();
		}

		[Test]
		public void ShouldGetPersonInfo_ExternalCalls()
		{
			var expected = new PersonInfo();
			var httpContext = new FakeHttpContext();
			httpContext.Items[TenantAuthentication.PersonInfoKey] = expected;

			new CurrentTenantUser(new FakeCurrentHttpContext(httpContext), null, null).CurrentUser()
					.Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldGetPersonInfo_InternalWebCalls()
		{
			var expected = new PersonInfo();
			var sessionData = new SessionSpecificData(Guid.NewGuid(), RandomName.Make(), Guid.NewGuid(), RandomName.Make());
			var sessionDataProvider = MockRepository.GenerateStub<ISessionSpecificDataProvider>();
			sessionDataProvider.Expect(x => x.GrabFromCookie()).Return(sessionData);
			var findPersonInfoByCredentials = MockRepository.GenerateMock<IFindPersonInfoByCredentials>();
			findPersonInfoByCredentials.Expect(x => x.Find(sessionData.PersonId, sessionData.TenantPassword)).Return(expected);
			var target = new CurrentTenantUser(new FakeCurrentHttpContext(new FakeHttpContext()), sessionDataProvider, findPersonInfoByCredentials);
			target.CurrentUser()
				.Should().Be.SameInstanceAs(expected);
		}
	}
}