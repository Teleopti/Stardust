using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class AuthenticationQuerierTest
	{
		[Test]
		public void DoApplicationLogonWithEncryptedNhibConfiguration()
		{
			var pathToTenantServer = RandomName.Make();
			var userAgent = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var applicationLogonClientModel = new ApplicationLogonClientModel();
			var applicationLogonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(applicationLogonClientModel)).Return(applicationLogonClientModelSerialized);
			var authResult = new AuthenticationQueryResult{DataSourceConfiguration = new DataSourceConfig()};
			postHttpRequest.Stub(
				x => x.Send<AuthenticationQueryResult>(pathToTenantServer + "Authenticate/ApplicationLogon", applicationLogonClientModelSerialized, userAgent))
				.Return(authResult);
			var nhibConfigEncryption = MockRepository.GenerateStub<INhibConfigEncryption>();
			nhibConfigEncryption.Stub(x => x.DecryptConfig(authResult.DataSourceConfiguration));
			var target = new AuthenticationQuerier(pathToTenantServer, nhibConfigEncryption, postHttpRequest, jsonSerializer);
			target.TryApplicationLogon(applicationLogonClientModel, userAgent)
				.Should().Be.SameInstanceAs(authResult);
		}

		[Test]
		public void DoIdentityLogonWithEncryptedNhibConfiguration()
		{
			var pathToTenantServer = RandomName.Make();
			var userAgent = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var logonClientModel = new IdentityLogonClientModel();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(logonClientModel)).Return(logonClientModelSerialized);
			var authResult = new AuthenticationQueryResult { DataSourceConfiguration = new DataSourceConfig() };
			postHttpRequest.Stub(
				x => x.Send<AuthenticationQueryResult>(pathToTenantServer + "Authenticate/IdentityLogon", logonClientModelSerialized, userAgent))
					.Return(authResult);
			var nhibConfigEncryption = MockRepository.GenerateStub<INhibConfigEncryption>();
			nhibConfigEncryption.Stub(x => x.DecryptConfig(authResult.DataSourceConfiguration));
			var target = new AuthenticationQuerier(pathToTenantServer, nhibConfigEncryption, postHttpRequest, jsonSerializer);
			target.TryIdentityLogon(logonClientModel, userAgent)
				.Should().Be.SameInstanceAs(authResult);
		}
	}
}