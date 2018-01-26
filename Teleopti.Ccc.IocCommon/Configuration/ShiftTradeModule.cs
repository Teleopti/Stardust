using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ShiftTradeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<IShiftTradeLightSpecification>();
			builder.RegisterType<ShiftTradeDateSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeTargetTimeSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeAbsenceSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradePersonalActivitySpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeMeetingSpecification>().As<IShiftTradeSpecification>();

			builder.RegisterType<ScheduleProjectionReadOnlyActivityProvider>()
			  .As<IScheduleProjectionReadOnlyActivityProvider>()
			  .SingleInstance();
			builder.RegisterType<ShiftTradeMaxSeatsSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeMaxSeatReadModelValidator>().As<IShiftTradeMaxSeatValidator>().SingleInstance();

			builder.RegisterType<SpecificationCheckerWithConfig>().As<ISpecificationChecker>();
			builder.RegisterType<ShiftTradeValidator>().As<IShiftTradeValidator>();
			builder.RegisterType<ShiftTradeLightValidator>().As<IShiftTradeLightValidator>();
			builder.RegisterType<ShiftTradeRequestSetChecksum>().As<IShiftTradeRequestSetChecksum>().SingleInstance();
			builder.RegisterType<ShiftTradeRequestStatusChecker>().As<IShiftTradeRequestStatusChecker>().SingleInstance();
			builder.RegisterType<ShiftTradeSwapScheduleDetailsMapper>().As<IShiftTradeSwapScheduleDetailsMapper>().SingleInstance();
			builder.RegisterType<ShiftTradePersonProvider>().As<IShiftTradePersonProvider>().SingleInstance();
		}
	}
}