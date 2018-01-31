using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class WebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<ScheduleDayProvider>().As<IScheduleDayProvider>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerChecker>().As<INonoverwritableLayerChecker>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerMovabilityChecker>().As<INonoverwritableLayerMovabilityChecker>().SingleInstance();
			builder.RegisterType<NonoverwritableLayerMovingHelper>().As<INonoverwritableLayerMovingHelper>().SingleInstance();
			builder.RegisterType<ScheduleProjectionHelper>().As<IScheduleProjectionHelper>().SingleInstance();
			builder.RegisterType<OrganizationSelectionProvider>().SingleInstance();
		}
	}
}