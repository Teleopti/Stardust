using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class WebModule : Module
	{
		private readonly IocConfiguration _configuration;

		public WebModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<ScheduleDayProvider>().As<IScheduleDayProvider>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerChecker>().As<INonoverwritableLayerChecker>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerMovabilityChecker>().As<INonoverwritableLayerMovabilityChecker>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerMovingHelper>().As<INonoverwritableLayerMovingHelper>().SingleInstance();
			if (_configuration.Toggle(Toggles.WfmTeamSchedule_SuggestShiftCategory_152))
			{
				builder.RegisterType<ShiftCategorySelector>().SingleInstance();
				builder.RegisterType<ShiftCategorySelectorWithPrediction>().As<IShiftCategorySelector>().SingleInstance();
			}
			else
			{
				builder.RegisterType<ShiftCategorySelector>().As<IShiftCategorySelector>().SingleInstance();
			}
			builder.RegisterType<ScheduleProjectionHelper>().As<IScheduleProjectionHelper>().SingleInstance();
			builder.RegisterType<OrganizationSelectionProvider>().SingleInstance();

			builder.RegisterType<HttpRequestHandler>().As<IHttpRequestHandler>();			
			builder.RegisterType<TokenGenerator>().As<ITokenGenerator>().SingleInstance();
			builder.RegisterType<SignatureCreator>().SingleInstance();
		}
	}
}