using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class AuthenticationQuerierTest
	{
		[Test]
		public void ShouldCallServerForApplicationLogon()
		{
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var converter = MockRepository.GenerateStub<IAuthenticationQuerierResultConverter>();
			var userAgent = RandomName.Make();
			var applicationLogonClientModel = new ApplicationLogonClientModel();
			var applicationLogonClientModelSerialized = RandomName.Make();
			var authQueryResult = new AuthenticationQueryResult();
			jsonSerializer.Expect(x => x.SerializeObject(applicationLogonClientModel)).Return(applicationLogonClientModelSerialized);
			postHttpRequest.Expect(
				x => x.Send<AuthenticationQueryResult>(pathToTenantServer + "Authenticate/ApplicationLogon", applicationLogonClientModelSerialized, userAgent))
				.Return(authQueryResult);
			var result = new AuthenticationQuerierResult();
			converter.Expect(x => x.Convert(authQueryResult)).Return(result);

			var target = new AuthenticationQuerier(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, converter);
			target.TryLogon(applicationLogonClientModel, userAgent)
				.Should().Be.SameInstanceAs(result);
		}

		[Test]
		public void ShouldCallServerForIdentityLogon()
		{
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var converter = MockRepository.GenerateStub<IAuthenticationQuerierResultConverter>();
			var userAgent = RandomName.Make();
			var identityLogonClientModel = new IdentityLogonClientModel();
			var identityLogonClientModelSerialized = RandomName.Make();
			var authQueryResult = new AuthenticationQueryResult();
			jsonSerializer.Expect(x => x.SerializeObject(identityLogonClientModel)).Return(identityLogonClientModelSerialized);
			postHttpRequest.Expect(
				x => x.Send<AuthenticationQueryResult>(pathToTenantServer + "Authenticate/IdentityLogon", identityLogonClientModelSerialized, userAgent))
				.Return(authQueryResult);
			var result = new AuthenticationQuerierResult();
			converter.Expect(x => x.Convert(authQueryResult)).Return(result);

			var target = new AuthenticationQuerier(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, converter);
			target.TryLogon(identityLogonClientModel, userAgent)
				.Should().Be.SameInstanceAs(result);
		}
	}
}