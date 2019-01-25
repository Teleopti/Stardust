using Autofac;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;
using Teleopti.Ccc.Sdk.WcfHost.Service.LogOn;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
	public class MultiTenancyModule :Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ChangePasswordTenantClient>().As<IChangePasswordTenantClient>().SingleInstance();
			builder.RegisterType<MultiTenancyAuthenticationFactory>().As<IAuthenticationFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TenantPeopleSaver>().As<ITenantPeopleSaver>().InstancePerLifetimeScope();
			builder.RegisterType<TenantDataManagerClient>().As<ITenantDataManagerClient>().InstancePerLifetimeScope();
			builder.RegisterType<SdkCurrentTenantCredentials>().As<ICurrentTenantCredentials>().InstancePerLifetimeScope();
			builder.RegisterType<CurrentPersonContainer>().As<ICurrentPersonContainer>();
			builder.RegisterType<TenantPeopleLoader>().As<ITenantPeopleLoader>().InstancePerLifetimeScope();
			builder.RegisterType<PersonDtoFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PersonCredentialsAppender>().InstancePerLifetimeScope();
		}
	}	
}
