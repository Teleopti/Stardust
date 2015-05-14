using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TestFixture]
	[TenantClientTest]
	public class MultiTenancyIdentityLogonTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public IMultiTenancyWindowsLogon Target;
		public FakePersonRepository PersonRepository;
		public RepositoryFactoryFake RepositoryFactoryFake;
		public FakeWindowUserProvider FakeWindowUserProvider;

		private const string userAgent = "something";
		

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSource.Expect(x => x.DataSourceName).Return("[not set]");
			var applicationData = new ApplicationDataFake();
			applicationData.SetDataSource(dataSource);
			var model = new LogonModel ();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personId = Guid.NewGuid();
			var person = new Person();
			person.SetId(personId);
			PersonRepository.Has(person);
			RepositoryFactoryFake.PersonRepository = PersonRepository;
			var dataSourceCfg = new DataSourceConfig
			{
				AnalyticsConnectionString = Encryption.EncryptStringToBase64("connstringtoencrypt", EncryptionConstants.Image1, EncryptionConstants.Image2),
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
			FakeWindowUserProvider.DomainName = "TOPTINET";
			FakeWindowUserProvider.UserName = "USER";
			dataSource.Stub(x => x.Application).Return(uowFactory);
			var result = Target.Logon(model,applicationData, userAgent);

			result.Successful.Should().Be.True();
			model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);

		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			var model = new LogonModel();
			var queryResult = new AuthenticationQueryResult
			{
				Success = false,
			};
			HttpRequestFake.SetReturnValue(queryResult);
			var result = Target.Logon(model, new ApplicationDataFake(), userAgent);

			result.Successful.Should().Be.False();
			model.SelectedDataSourceContainer.Should().Be.Null();
		}
	}

	public class FakeWindowUserProvider : IWindowsUserProvider 
	{
		public string DomainName { get;  set; }
		public string UserName { get;  set; }
	}
}