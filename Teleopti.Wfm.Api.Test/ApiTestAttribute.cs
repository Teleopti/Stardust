﻿using System;
using Autofac;
using Microsoft.Owin.Testing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test
{
	public class ApiTestAttribute : DomainTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);

			extend.AddModule(new ApiModule());
			extend.AddModule(new clientModule());
		}

		private class clientModule : Module
		{
			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterType<Startup>();
				builder.RegisterType<clientFactory>();
				builder.RegisterAdapter<clientFactory,IApiHttpClient>((b,c) => c.createClient());
			}

			private class clientFactory
			{
				private readonly Startup _startup;

				public clientFactory(Startup startup)
				{
					_startup = startup;
				}

				public IApiHttpClient createClient()
				{
					var server = TestServer.Create(x => _startup.Configuration(x));
					return new ApiHttpClient(server.HttpClient);
				}
			}


		}



		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble(new sharedSettingsQuerierFake()).For<ISharedSettingsQuerier>();
			var requestFake = new PostHttpRequestFake();
			var nhibDecryption = new DataSourceConfigDecryption();
			requestFake.SetReturnValue(new AuthenticationInternalQuerierResult
			{
				Success = true,
				PersonId = FakeTokenVerifier.UserId,
				Tenant = DomainTestAttribute.DefaultTenantName,
				DataSourceConfiguration =
					nhibDecryption.EncryptConfigJustForTest(new DataSourceConfig
					{
						AnalyticsConnectionString = "",
						ApplicationConnectionString = ""
					})
			});
			isolate.UseTestDouble(requestFake).For<IPostHttpRequest>();

			isolate.UseTestDouble<FakeTokenVerifier>().For<ITokenVerifier>();
			var person = PersonFactory.CreatePerson().WithId(FakeTokenVerifier.UserId);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var userUnauthorized = new LoadUserUnauthorizedFake();
			userUnauthorized.Has(person);

			var tenants = new FakeTenants();
			tenants.Has(DomainTestAttribute.DefaultTenantName, LegacyAuthenticationKey.TheKey);
			isolate.UseTestDouble(tenants).For<IFindTenantNameByRtaKey>();
			isolate.UseTestDouble(tenants).For<ICountTenants>();
			isolate.UseTestDouble(tenants).For<ILoadAllTenants>();
			isolate.UseTestDouble(tenants).For<IFindTenantByName>();
			isolate.UseTestDouble(tenants).For<IAllTenantNames>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			isolate.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			isolate.UseTestDouble<FakeAllLicenseActivatorProvider>().For<ILicenseActivatorProvider>();
			isolate.UseTestDouble(userUnauthorized).For<ILoadUserUnauthorized>();
			
			var businessUnitRepository = new FakeBusinessUnitRepository();
			businessUnitRepository.Has(BusinessUnitFactory.BusinessUnitUsedInTest);

			var fakeStorage = new FakeStorage();
			var personRepository = new FakePersonRepository(fakeStorage);
			personRepository.Has(person);

			isolate.UseTestDouble(fakeStorage).For<IFakeStorage>();
			isolate.UseTestDouble(personRepository).For<IPersonRepository>();
			isolate.UseTestDouble(personRepository).For<IProxyForId<IPerson>>();
			isolate.UseTestDouble(personRepository).For<IPersonLoadAllWithPeriodAndExternalLogOn>();
			isolate.UseTestDouble(businessUnitRepository).For<IBusinessUnitRepository>();
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
