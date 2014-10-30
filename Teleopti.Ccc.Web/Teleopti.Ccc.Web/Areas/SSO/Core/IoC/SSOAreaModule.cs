﻿using Autofac;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.SSO.Core.IoC
{
	public class SSOAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OpenIdProviderWapper>().As<IOpenIdProviderWapper>().SingleInstance();
			builder.RegisterType<ApplicationAuthenticationType>().As<IApplicationAuthenticationType>().SingleInstance();
			builder.RegisterType<ApplicationDataSourcesViewModelFactory>().As<IApplicationDataSourcesViewModelFactory>().SingleInstance();
			builder.RegisterType<ProviderEndpointWrapper>().As<IProviderEndpointWrapper>().SingleInstance();
			builder.RegisterType<OpenIdProvider>().SingleInstance();
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<ApplicationAuthenticationType>().As<IApplicationAuthenticationType>().SingleInstance();
		}
	}
}
