using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class LegacyRegistrationsFromAnywhere : Module
	{
		private readonly IocConfiguration _configuration;

		public LegacyRegistrationsFromAnywhere(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();

			if (!_configuration.Args().DisableWebSocketCors)
			{
				builder.RegisterType<OriginHandlerPipelineModule>().As<IHubPipelineModule>();
			}

			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<PersonScheduleViewModelMapper>().SingleInstance();

			builder.Register(c => new ReportUrlConstructor(_configuration.Args().ReportServer, c.Resolve<IConfigReader>()))
				.As<IReportUrl>()
				.SingleInstance();
		}
	}
}