using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	public class MultiTenancyApplicationLogonTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public IMultiTenancyApplicationLogon Target;
		public LoadUserUnauthorizedFake LoadUserUnauthorized;

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			LoadUserUnauthorized.Has(person);
			var dataSourceCfg = new DataSourceConfig
			{
				AnalyticsConnectionString = Encryption.EncryptStringToBase64("constrringtoencrypt", EncryptionConstants.Image1, EncryptionConstants.Image2),
				ApplicationNHibernateConfig = new Dictionary<string, string>()
			};

			var queryResult = new AuthenticationQueryResult
			{
				PersonId = personId,
				Success = true,
				Tenant = "[not set]",
				DataSourceConfiguration = dataSourceCfg
			};
			HttpRequestFake.SetReturnValue(queryResult);
			
			var result = Target.Logon(RandomName.Make(), RandomName.Make(), RandomName.Make());

			result.Successful.Should().Be.True();
			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			var queryResult = new AuthenticationQueryResult { Success = false };
			HttpRequestFake.SetReturnValue(queryResult);
			var result = Target.Logon(RandomName.Make(), RandomName.Make(), RandomName.Make());

			result.Successful.Should().Be.False();
			result.Person.Should().Be.Null();
		}
	}
}