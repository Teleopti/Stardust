using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	public class MultiTenancyIdentityLogonTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public IMultiTenancyWindowsLogon Target;
		public FakeWindowUserProvider FakeWindowUserProvider;
		public LoadUserUnauthorizedFake LoadUserUnauthorized;

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			LoadUserUnauthorized.Has(person);

			var queryResult = new AuthenticationInternalQuerierResult
			{
				PersonId = personId,
				Success = true,
				Tenant = "[not set]",
				DataSourceConfiguration = new DataSourceConfig
				{
					AnalyticsConnectionString =
						Encryption.EncryptStringToBase64("connstringtoencrypt", EncryptionConstants.Image1, EncryptionConstants.Image2),
					ApplicationNHibernateConfig = new Dictionary<string, string>()
				}
			};
			HttpRequestFake.SetReturnValue(queryResult);
			FakeWindowUserProvider.SetIdentity("TOPTINET\\USER");
			var result = Target.Logon(RandomName.Make());

			result.Successful.Should().Be.True();
			result.Person.Should().Be.SameInstanceAs(person);

		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			var model = new LogonModel();
			var queryResult = new AuthenticationInternalQuerierResult
			{
				Success = false,
			};
			HttpRequestFake.SetReturnValue(queryResult);
			var result = Target.Logon(RandomName.Make());

			result.Successful.Should().Be.False();
			model.SelectedDataSourceContainer.Should().Be.Null();
		}
	}

	public class FakeWindowUserProvider : IWindowsUserProvider
	{
		private string _identity;

		public void SetIdentity(string identity)
		{
			_identity = identity;
		}

		public string Identity()
		{
			return _identity;
		}
	}
}