using System;
using System.Net.Http;
using Autofac;
using Microsoft.Owin.Testing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test
{
	public abstract class ApiTest
	{
		private readonly TestServer _server;

		protected HttpClient Client { get; }

		protected ApiTest()
		{
			var startup = new Startup(testRegistrations);
			_server = TestServer.Create(x => startup.Configuration(x));
			Client = _server.HttpClient;
		}

		private void testRegistrations(ContainerBuilder builder)
		{
			var person = PersonFactory.CreatePerson().WithId(FakeTokenVerifier.UserId);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var userUnauthorized = new LoadUserUnauthorizedFake();
			userUnauthorized.Has(person);

			builder.RegisterType<FakeTokenVerifier>().As<ITokenVerifier>();

			var tenants = new FakeTenants();
			tenants.Has(DomainTestAttribute.DefaultTenantName, LegacyAuthenticationKey.TheKey);
			builder.RegisterInstance(tenants).As<IFindTenantNameByRtaKey>().As<ICountTenants>().As<ILoadAllTenants>().As<IFindTenantByName>().As<IAllTenantNames>();
			builder.RegisterType<TenantAuthenticationFake>().As<ITenantAuthentication>();
			builder.RegisterType<TenantUnitOfWorkFake>().As<ITenantUnitOfWork>();
			builder.RegisterType<FakeDataSourceForTenant>().As<IDataSourceForTenant>();
			builder.RegisterType<FakeDataSourcesFactory>().As<IDataSourcesFactory>();
			builder.RegisterType<FakeAllLicenseActivatorProvider>().As<ILicenseActivatorProvider>();
			builder.RegisterInstance(userUnauthorized).As<ILoadUserUnauthorized>();

			var businessUnitRepository = new FakeBusinessUnitRepository();
			businessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);

			var fakeStorage = new FakeStorage();
			var personRepository = new FakePersonRepository(fakeStorage);
			personRepository.Has(person);

			builder.RegisterInstance(fakeStorage).As<IFakeStorage>();
			builder.RegisterType<FakeRepositoryFactory>().As<IRepositoryFactory>();
			builder.RegisterType<FakeRepositoryFactory>().As<IRepositoryFactory>();
			builder.RegisterType<FakeLicenseRepository>().As<ILicenseRepository>().As<ILicenseRepositoryForLicenseVerifier>();
			builder.RegisterInstance(personRepository).As<IPersonRepository>().As<IProxyForId<IPerson>>().As<IPersonLoadAllWithPeriodAndExternalLogOn>();
			builder.RegisterInstance(businessUnitRepository).As<IBusinessUnitRepository>();

			builder.RegisterInstance(new sharedSettingsQuerierFake()).As<ISharedSettingsQuerier>();
		}

		protected void Authorize()
		{
			Client.DefaultRequestHeaders.Add("Authorization", "bearer afdsafasdf");
		}

		private class sharedSettingsQuerierFake : ISharedSettingsQuerier
		{
			public SharedSettings GetSharedSettings()
			{
				return new SharedSettings();
			}
		}
	}
}
