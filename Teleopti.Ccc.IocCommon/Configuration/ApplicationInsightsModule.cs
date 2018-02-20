using Autofac;
using Microsoft.ApplicationInsights;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ApplicationInsightsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationInsigths>()
				.SingleInstance()
				.As<IApplicationInsights>();
			builder.RegisterType<TelemetryClient>()
				.SingleInstance()
				.AsSelf();
			builder.RegisterType<ApplicationInsightsConfigurationReader>()
				.SingleInstance()
				.As<IApplicationInsightsConfigurationReader>();
		}
	}
}