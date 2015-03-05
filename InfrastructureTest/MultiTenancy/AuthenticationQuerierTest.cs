using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class AuthenticationQuerierTest
	{
		[Test]
		public void DoApplicationLogonWithEncryptedNhibConfiguration()
		{
			var userName = RandomName.Make();
			var password = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var userAgent = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var authResult = new AuthenticationQueryResult{DataSourceConfiguration = new DataSourceConfig()};
			postHttpRequest.Stub(
				x =>
					x.Send<AuthenticationQueryResult>(Arg.Is(pathToTenantServer + "Authenticate/ApplicationLogon"), Arg.Is(userAgent),
						Arg<IEnumerable<KeyValuePair<string, string>>>.List.ContainsAll(new Dictionary<string, string>
						{
							{"userName", userName},
							{"password", password}
						}))).Return(authResult);
			var nhibConfigEncryption = MockRepository.GenerateStub<INhibConfigEncryption>();
			nhibConfigEncryption.Stub(x => x.DecryptConfig(authResult.DataSourceConfiguration));
			var target = new AuthenticationQuerier(pathToTenantServer, nhibConfigEncryption, postHttpRequest);
			target.TryLogon(userName, password, userAgent)
				.Should().Be.SameInstanceAs(authResult);
		}

		[Test]
		public void DoIdentityLogonWithEncryptedNhibConfiguration()
		{
			var identity = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var userAgent = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var authResult = new AuthenticationQueryResult { DataSourceConfiguration = new DataSourceConfig() };
			postHttpRequest.Stub(
				x =>
					x.Send<AuthenticationQueryResult>(Arg.Is(pathToTenantServer + "Authenticate/IdentityLogon"), Arg.Is(userAgent),
						Arg<IEnumerable<KeyValuePair<string, string>>>.List.ContainsAll(new Dictionary<string, string>
						{
							{"identity", identity}
						}))).Return(authResult);
			var nhibConfigEncryption = MockRepository.GenerateStub<INhibConfigEncryption>();
			nhibConfigEncryption.Stub(x => x.DecryptConfig(authResult.DataSourceConfiguration));
			var target = new AuthenticationQuerier(pathToTenantServer, nhibConfigEncryption, postHttpRequest);
			target.TryIdentityLogon(identity, userAgent)
				.Should().Be.SameInstanceAs(authResult);
		}
	}
}