using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class TenantModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public TenantModule(IIocConfiguration configuration) 
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var tenantServer = _configuration.Args().TenantServer;
			builder.Register(c => new TenantServerConfiguration(tenantServer)).As<ITenantServerConfiguration>().SingleInstance();
			builder.RegisterType<AuthenticationQuerierResultConverter>()
				.As<IAuthenticationQuerierResultConverter>()
				.SingleInstance();
			if (isRunFromTest(tenantServer) || tenantServer.IsAnUrl())
			{
				builder.RegisterType<AuthenticationQuerier>().As<IAuthenticationQuerier>().SingleInstance();
				builder.RegisterType<TenantLogonDataManager>().As<ITenantLogonDataManager>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AuthenticationFromFileQuerier>().As<IAuthenticationQuerier>().SingleInstance();
				// must still register this to work from sikuli
				builder.RegisterType<emptyTenantLogonDataManager>().As<ITenantLogonDataManager>().SingleInstance();
			}
			builder.RegisterType<PostHttpRequest>().As<IPostHttpRequest>().SingleInstance();
			builder.RegisterType<GetHttpRequest>().As<IGetHttpRequest>().SingleInstance();
			builder.RegisterType<DataSourceConfigDecryption>().As<IDataSourceConfigDecryption>().SingleInstance();

			var configServer = _configuration.Args().ConfigServer;
			if (isRunFromTest(configServer) || configServer.IsAnUrl())
			{
				builder.Register(c => new SharedSettingsQuerier(configServer))
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
			else
			{
				builder.Register(c => new SharedSettingsQuerierForNoWeb(configServer))
					.As<ISharedSettingsQuerier>()
					.SingleInstance();
			}
			builder.RegisterType<ChangePassword>().As<IChangePassword>().SingleInstance();
			builder.RegisterType<ResponseException>().As<IResponseException>();
			builder.RegisterType<TenantDataManager>().As<ITenantDataManager>().SingleInstance();

			if (_configuration.Args().BehaviorTestServer)
				builder.RegisterType<BehaviorTestTenants>().As<IAllTenantNames>().SingleInstance();
			else
				builder.RegisterType<CannotIterateAllTenants>().As<IAllTenantNames>().SingleInstance();

			fromServerModule(builder);

			builder.RegisterType<FalsePryl>().As<ITenantAuthentication>().SingleInstance();

		}

		private class FalsePryl : ITenantAuthentication
		{
			public bool Logon()
			{
				return false;
			}
		}

		private void fromServerModule(ContainerBuilder builder)
		{
			builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			builder.RegisterType<FindPersonInfo>().As<IFindPersonInfo>().SingleInstance();
			builder.RegisterType<FindPersonInfoByCredentials>().As<IFindPersonInfoByCredentials>().SingleInstance();
			builder.RegisterType<FindTenantNameByRtaKey>().As<IFindTenantNameByRtaKey>().SingleInstance();
			builder.RegisterType<FindTenantByName>().As<IFindTenantByName>().SingleInstance();
			builder.RegisterType<FindTenantByNameWithEnsuredTransaction>().As<IFindTenantByNameWithEnsuredTransaction>().SingleInstance();
			builder.RegisterType<CountTenants>().As<ICountTenants>().SingleInstance();

			builder.RegisterType<FindPersonInfoByIdentity>().As<IFindPersonInfoByIdentity>().SingleInstance();
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connstringAsString = configReader.ConnectionString("Tenancy");
				return TenantUnitOfWorkManager.Create(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<TenantUnitOfWorkAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<WithTenantUnitOfWork>().SingleInstance();
			builder.RegisterType<PersistLogonAttempt>().As<IPersistLogonAttempt>().SingleInstance();
			builder.RegisterType<PersistPersonInfo>().As<IPersistPersonInfo>().SingleInstance();
			builder.RegisterType<PersonInfoCreator>().As<IPersonInfoCreator>().SingleInstance();
			builder.RegisterType<PersonInfoHelper>().As<IPersonInfoHelper>().SingleInstance();
			builder.RegisterType<DeletePersonInfo>().As<IDeletePersonInfo>().SingleInstance();
			builder.RegisterType<VerifyPasswordPolicy>().As<IVerifyPasswordPolicy>().SingleInstance();
			builder.RegisterType<CheckPasswordStrength>().As<ICheckPasswordStrength>().SingleInstance();
			builder.RegisterType<CurrentTenant>().As<ICurrentTenant>().SingleInstance();
			builder.RegisterType<FindLogonInfo>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
			builder.RegisterType<ServerConfigurationRepository>().As<IServerConfigurationRepository>().SingleInstance();
			builder.RegisterType<TenantExists>().As<ITenantExists>().SingleInstance();
			builder.RegisterType<LoadAllPersonInfos>().SingleInstance();
			builder.RegisterType<PersistTenant>().SingleInstance();
			builder.RegisterType<DeleteTenant>().SingleInstance();
			builder.RegisterType<CheckTenantUserExists>().As<ICheckTenantUserExists>().SingleInstance();

			builder.RegisterType<PersistExternalApplicationAccess>().SingleInstance();
			builder.RegisterType<FindExternalApplicationAccessByHash>().As<IFindExternalApplicationAccessByHash>().SingleInstance();
		}

		private static bool isRunFromTest(string server)
		{
			return server == null;
		}

		private class emptyTenantLogonDataManager : ITenantLogonDataManager
		{
			public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
			{
				return Enumerable.Empty<LogonInfoModel>();
			}

			public LogonInfoModel GetLogonInfoForLogonName(string logonName)
			{
				throw new NotImplementedException();
			}

			public LogonInfoModel GetLogonInfoForIdentity(string identity)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<LogonInfoModel> GetLogonInfoForIdentities(IEnumerable<string> identities)
			{
				throw new NotImplementedException();
			}
		}
	}
}