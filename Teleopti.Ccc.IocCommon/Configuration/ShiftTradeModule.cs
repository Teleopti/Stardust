﻿using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	//has a dependency to ISchedulingResultStateHolder that should be more than nice to get rid of
	public class ShiftTradeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>();

			builder.RegisterType<ShiftTradeSkillSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeTargetTimeSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeAbsenceSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradePersonalActivitySpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeMeetingSpecification>().As<IShiftTradeSpecification>();

			builder.RegisterType<ShiftTradeValidator>().As<IShiftTradeValidator>();
		}
	}
}