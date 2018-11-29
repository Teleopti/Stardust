using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class AuthenticationQuerierResultConverterTest
	{
		[Test]
		public void VerifyUnsuccesful()
		{
			var failReason = RandomName.Make();
			var target = new AuthenticationQuerierResultConverter(null, null, null);
			var result = target.Convert(new AuthenticationInternalQuerierResult { Success = false, FailReason = failReason });
			shouldBeUnsuccesful(result);
			result.FailReason.Should().Be.EqualTo(failReason);
		}

		[Test]
		public void ShouldDecryptNhibConfigIfSuccessful()
		{
			var dataSourceForTenant = MockRepository.GenerateMock<IDataSourceForTenant>();
			var nhibDecryption = new DataSourceConfigDecryption();
			var loadUnauthorizedUserDoesntMatter = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			loadUnauthorizedUserDoesntMatter.Expect(x => x.LoadFullPersonInSeperateTransaction(null, Guid.Empty)).IgnoreArguments().Return(new Person());
			var target = new AuthenticationQuerierResultConverter(nhibDecryption, () => dataSourceForTenant, loadUnauthorizedUserDoesntMatter);
			var analyticsdb = RandomName.Make();
			var appdb = new Dictionary<string, string> { { "test", RandomName.Make() } };
			var tenantName = RandomName.Make();
			dataSourceForTenant.Expect(x => x.Tenant(tenantName)).Return(new FakeDataSource());
			var appConnString = RandomName.Make();
			var result = target.Convert(new AuthenticationInternalQuerierResult
				{
					Tenant = tenantName,
					Success = true,
					DataSourceConfiguration = nhibDecryption.EncryptConfigJustForTest(new DataSourceConfig {AnalyticsConnectionString = analyticsdb, ApplicationConnectionString = appConnString}),
					ApplicationConfig = appdb
			});
			result.Success.Should().Be.True();
			dataSourceForTenant.AssertWasCalled(x => x.MakeSureDataSourceCreated(tenantName, appConnString, analyticsdb, appdb));
		}

		[Test]
		public void ShouldSetPersonIfSuccessful()
		{
			var personId = Guid.NewGuid();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dataSource = new FakeDataSource{Application = uowFactory, DataSourceName = RandomName.Make()};
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, null);
			dataSourceForTenant.Has(dataSource);
			var loadUser = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			var person = new Person();
			loadUser.Expect(x => x.LoadFullPersonInSeperateTransaction(uowFactory, personId)).Return(person);
				
			var target = new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(), () => dataSourceForTenant, loadUser);
			var result = target.Convert(new AuthenticationInternalQuerierResult
			{
				Success = true, 
				PersonId = personId,
				DataSourceConfiguration = createFakeConfig(),
				Tenant = dataSource.DataSourceName
			});
			result.Success.Should().Be.True();
			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldSetDatasourceIfSuccessful()
		{
			var loadUnauthorizedUserDoesntMatter = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			loadUnauthorizedUserDoesntMatter.Expect(x => x.LoadFullPersonInSeperateTransaction(null, Guid.Empty)).IgnoreArguments().Return(new Person());
			var dataSource = new FakeDataSource {DataSourceName = RandomName.Make()};
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, null);
			dataSourceForTenant.Has(dataSource);

			var target = new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(), () => dataSourceForTenant, loadUnauthorizedUserDoesntMatter);
			var result = target.Convert(new AuthenticationInternalQuerierResult
			{
				Success = true,
				DataSourceConfiguration = createFakeConfig(),
				Tenant = dataSource.DataSourceName
			});
			result.Success.Should().Be.True();
			result.DataSource.Should().Be.SameInstanceAs(dataSource);
		}


		[Test]
		public void ShouldSetTenantPasswordIfSuccessful()
		{
			var loadUnauthorizedUserDoesntMatter = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			loadUnauthorizedUserDoesntMatter.Expect(x => x.LoadFullPersonInSeperateTransaction(null, Guid.Empty)).IgnoreArguments().Return(new Person());
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, null);
			dataSourceForTenant.Has(dataSource);
			var tenantPassword = RandomName.Make();

			var target = new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(), () => dataSourceForTenant, loadUnauthorizedUserDoesntMatter);
			var result = target.Convert(new AuthenticationInternalQuerierResult
			{
				Success = true,
				DataSourceConfiguration = createFakeConfig(),
				TenantPassword = tenantPassword,
				Tenant = dataSource.DataSourceName
			});
			result.Success.Should().Be.True();
			result.TenantPassword.Should().Be.EqualTo(tenantPassword);
		}

		[Test]
		public void ShouldBeUnsuccesfulIfTerminalDateHasPassed()
		{
			var personId = Guid.NewGuid();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dataSource = new FakeDataSource { Application = uowFactory, DataSourceName = RandomName.Make()};
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, null);
			dataSourceForTenant.Has(dataSource);
			var loadUser = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			var person = new Person();
			person.TerminatePerson(DateOnly.Today.AddDays(-1), new PersonAccountUpdaterDummy());
			loadUser.Expect(x => x.LoadFullPersonInSeperateTransaction(uowFactory, personId)).Return(person);

			var target = new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(), () => dataSourceForTenant, loadUser);
			var result = target.Convert(new AuthenticationInternalQuerierResult
			{
				Success = true,
				PersonId = personId,
				DataSourceConfiguration = createFakeConfig(),
				Tenant = dataSource.DataSourceName
			});
			shouldBeUnsuccesful(result);
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldBeSuccesfulIfTerminalDateIsToday()
		{
			var personId = Guid.NewGuid();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dataSource = new FakeDataSource { Application = uowFactory, DataSourceName = RandomName.Make() };
			var dataSourceForTenant = new FakeDataSourceForTenant(null, null, null);
			dataSourceForTenant.Has(dataSource);
			var loadUser = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			var person = new Person();
			person.TerminatePerson(DateOnly.Today, new PersonAccountUpdaterDummy());
			loadUser.Expect(x => x.LoadFullPersonInSeperateTransaction(uowFactory, personId)).Return(person);

			var target = new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(), () => dataSourceForTenant, loadUser);
			var result = target.Convert(new AuthenticationInternalQuerierResult
			{
				Success = true,
				PersonId = personId,
				DataSourceConfiguration = createFakeConfig(),
				Tenant = dataSource.DataSourceName
			});
			result.Success.Should().Be.True();
		}

		private static void shouldBeUnsuccesful(AuthenticationQuerierResult result)
		{
			result.Success.Should().Be.False();
			result.DataSource.Should().Be.Null();
			result.Person.Should().Be.Null();
		}

		private static DataSourceConfig createFakeConfig()
		{
			return new DataSourceConfigDecryption().EncryptConfigJustForTest(new DataSourceConfig
			{
				AnalyticsConnectionString = RandomName.Make()
			});
		}
	}
}