using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class TenantClientModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public TenantClientModule(IIocConfiguration configuration) 
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var tenantServer = _configuration.Args().TenantServer;
			builder.Register(c => new TenantServerConfiguration(tenantServer)).As<ITenantServerConfiguration>().SingleInstance();
			builder.RegisterType<AuthenticationQuerierResultConverter>().As<IAuthenticationQuerierResultConverter>().SingleInstance();
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
			if(isRunFromTest(configServer) || configServer.IsAnUrl())
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

			builder.RegisterType<CannotIterateAllTenants>().As<IAllTenantNames>().SingleInstance();
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
		}
	}
}