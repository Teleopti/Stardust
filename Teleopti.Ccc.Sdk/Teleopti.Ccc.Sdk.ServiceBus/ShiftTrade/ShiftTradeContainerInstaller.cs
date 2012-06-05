using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.ShiftTrade
{
    public class ShiftTradeContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ShiftTradeSkillSpecification>().As<IShiftTradeSkillSpecification>();
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<IOpenShiftTradePeriodSpecification>();
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>();
			builder.RegisterType<ShiftTradeTargetTimeSpecification>().As<IShiftTradeTargetTimeSpecification>();
			builder.RegisterType<IsWorkflowControlSetNullSpecification>().As<IIsWorkflowControlSetNullSpecification>();
			builder.RegisterType<ShiftTradeAbsenceSpecification>().As<IShiftTradeAbsenceSpecification>();
			builder.RegisterType<ShiftTradePersonalActivitySpecification>().As<IShiftTradePersonalActivitySpecification>();
			builder.RegisterType<ShiftTradeMeetingSpecification>().As<IShiftTradeMeetingSpecification>();
			builder.RegisterType<ShiftTradeValidator>().As<IShiftTradeValidator>();
		}
    }
}