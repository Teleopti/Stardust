﻿using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class TenantServerModule : Module
	{
		private readonly IIocConfiguration _iocConfiguration;

		public TenantServerModule(IIocConfiguration iocConfiguration)
		{
			_iocConfiguration = iocConfiguration;
		}

		private const string tenancyConnectionStringKey = "Tenancy";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IdentityUserQuery>().As<IIdentityUserQuery>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
			builder.RegisterType<FindPersonInfo>().As<IFindPersonInfo>().SingleInstance();
			builder.RegisterType<FindPersonInfoByCredentials>().As<IFindPersonInfoByCredentials>().SingleInstance();

			builder.RegisterType<FindPersonInfoByIdentity>().As<IFindPersonInfoByIdentity>().SingleInstance();
			builder.Register(c =>
			{
				var configReader = c.Resolve<IConfigReader>();
				var connStringToTenant = configReader.ConnectionStrings[tenancyConnectionStringKey];
				var connstringAsString = connStringToTenant == null ? null : connStringToTenant.ConnectionString;
				return TenantUnitOfWorkManager.CreateInstanceForWeb(connstringAsString);
			})
				.As<ITenantUnitOfWork>()
				.As<ICurrentTenantSession>()
				.SingleInstance();
			builder.RegisterType<TenantUnitOfWorkAspect>().As<ITenantUnitOfWorkAspect>().SingleInstance();
			builder.RegisterType<PersistLogonAttempt>().As<IPersistLogonAttempt>().SingleInstance();
			if (_iocConfiguration.Toggle(Toggles.Tenant_RemoveNhibFiles_33685))
			{
				builder.RegisterType<ReadDataSourceConfiguration>().As<IReadDataSourceConfiguration>().SingleInstance();
			}
			else
			{
				builder.RegisterType<ReadDataSourceConfigurationFromNhibFiles>().As<IReadDataSourceConfiguration>().SingleInstance();
			}
			builder.RegisterType<PersistPersonInfo>().As<IPersistPersonInfo>().SingleInstance();
			builder.RegisterType<DeletePersonInfo>().As<IDeletePersonInfo>().SingleInstance();
			builder.RegisterType<VerifyPasswordPolicy>().As<IVerifyPasswordPolicy>().SingleInstance();
			builder.RegisterType<CheckPasswordStrength>().As<ICheckPasswordStrength>().SingleInstance();
			builder.RegisterType<CurrentTenant>().As<ICurrentTenant>().SingleInstance();
			builder.RegisterType<FindLogonInfo>().As<IFindLogonInfo>().SingleInstance();
			builder.RegisterType<LoadAllTenants>().As<ILoadAllTenants>().SingleInstance();
			builder.RegisterType<TenantExists>().As<ITenantExists>().SingleInstance();
			builder.RegisterType<LoadAllPersonInfos>().SingleInstance();
		}
	}
}