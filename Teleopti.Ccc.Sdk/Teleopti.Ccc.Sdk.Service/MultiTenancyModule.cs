﻿using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService
{
	public class MultiTenancyModule :Module
	{
		private readonly IIocConfiguration _configuration;

		public MultiTenancyModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ChangePassword>().As<IChangePassword>().SingleInstance();
			builder.RegisterType<MultiTenancyAuthenticationFactory>().As<IAuthenticationFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TenantPeopleSaver>().As<ITenantPeopleSaver>().InstancePerLifetimeScope();
			builder.RegisterType<TenantDataManager>().As<ITenantDataManager>().InstancePerLifetimeScope();

			if (_configuration.Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049))
				builder.RegisterType<TenantPeopleLoader>().As<ITenantPeopleLoader>().InstancePerLifetimeScope();
			else
				builder.RegisterType<emptyTenantPeopleLoader>().As<ITenantPeopleLoader>().InstancePerLifetimeScope();
		}
	}	
}
