using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class LoginAttemptModelFactoryForWebTest
	{
		[Test]
		public void SuccessfulApplicationLogon()
		{
			const string userName = "userName";
			Guid? personId = Guid.NewGuid();
			const string ipadress = "1.2.3.4";
			var ipAdress = MockRepository.GenerateStub<IIpAddressResolver>();
			ipAdress.Expect(x => x.GetIpAddress()).Return(ipadress);
			var userAgent = Guid.NewGuid().ToString();

			var target = new LoginAttemptModelFactoryForWeb(null, ipAdress, new HttpRequestUserAgentFake(userAgent));
			var result = target.Create(userName, personId, true);
			result.Client.Should().Be.EqualTo(userAgent);
			result.ClientIp.Should().Be.EqualTo(ipadress);
			result.PersonId.Should().Be.EqualTo(personId);
			result.Provider.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.ApplicationProvider);
			result.Result.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.SuccessfulLogon);
			result.UserCredentials.Should().Be.EqualTo(userName);
		}

		[Test]
		public void FailedApplicationLogon()
		{
			const string userName = "user Name";
			Guid? personId = Guid.NewGuid();
			const string ipadress = "4.2.3.4";
			var ipAdress = MockRepository.GenerateStub<IIpAddressResolver>();
			ipAdress.Expect(x => x.GetIpAddress()).Return(ipadress);
			var userAgent = Guid.NewGuid().ToString();

			var target = new LoginAttemptModelFactoryForWeb(null, ipAdress, new HttpRequestUserAgentFake(userAgent));
			var result = target.Create(userName, personId, false);
			result.Client.Should().Be.EqualTo(userAgent);
			result.ClientIp.Should().Be.EqualTo(ipadress);
			result.PersonId.Should().Be.EqualTo(personId);
			result.Provider.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.ApplicationProvider);
			result.Result.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.FailedLogon);
			result.UserCredentials.Should().Be.EqualTo(userName);
		}

		[Test]
		public void SuccessfulWindowsLogon()
		{
			const string winUser= "userName";
			Guid? personId = Guid.NewGuid();
			const string ipadress = "1.2.3.4";
			var ipAdress = MockRepository.GenerateStub<IIpAddressResolver>();
			ipAdress.Expect(x => x.GetIpAddress()).Return(ipadress);
			var identityProvider = MockRepository.GenerateStub<ITokenIdentityProvider>();
			var tokenIdentity = new TokenIdentity {UserIdentifier = winUser};
			identityProvider.Expect(x => x.RetrieveToken()).Return(tokenIdentity);
			var userAgent = Guid.NewGuid().ToString();

			var target = new LoginAttemptModelFactoryForWeb(null, ipAdress, new HttpRequestUserAgentFake(userAgent));
			var result = target.Create(null, personId, true);
			result.Client.Should().Be.EqualTo(userAgent);
			result.ClientIp.Should().Be.EqualTo(ipadress);
			result.PersonId.Should().Be.EqualTo(personId);
			result.Provider.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.WindowsProvider);
			result.Result.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.SuccessfulLogon);
			result.UserCredentials.Should().Be.EqualTo(winUser);
		}

		[Test]
		public void FailedWindowsLogon()
		{
			const string winUser = "user Name";
			Guid? personId = Guid.NewGuid();
			const string ipadress = "4.2.3.4";
			var ipAdress = MockRepository.GenerateStub<IIpAddressResolver>();
			ipAdress.Expect(x => x.GetIpAddress()).Return(ipadress);
			var identityProvider = MockRepository.GenerateStub<ITokenIdentityProvider>();
			var tokenIdentity = new TokenIdentity { UserIdentifier = winUser };
			identityProvider.Expect(x => x.RetrieveToken()).Return(tokenIdentity);
			var userAgent = Guid.NewGuid().ToString();

			var target = new LoginAttemptModelFactoryForWeb(null, ipAdress, new HttpRequestUserAgentFake(userAgent));
			var result = target.Create(null, personId, false);
			result.Client.Should().Be.EqualTo(userAgent);
			result.ClientIp.Should().Be.EqualTo(ipadress);
			result.PersonId.Should().Be.EqualTo(personId);
			result.Provider.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.WindowsProvider);
			result.Result.Should().Be.EqualTo(LoginAttemptModelFactoryForWeb.FailedLogon);
			result.UserCredentials.Should().Be.EqualTo(winUser);
		}
	}
}