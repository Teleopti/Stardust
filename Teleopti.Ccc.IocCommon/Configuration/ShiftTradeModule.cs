﻿using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class ShiftTradeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().SingleInstance();

			builder.RegisterType<ShiftTradeSkillSpecification>().As<IShiftTradeLightSpecification>();
			builder.RegisterType<OpenShiftTradePeriodSpecification>().As<IShiftTradeLightSpecification>();

			builder.RegisterType<ShiftTradeDateSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeTargetTimeSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeAbsenceSpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradePersonalActivitySpecification>().As<IShiftTradeSpecification>();
			builder.RegisterType<ShiftTradeMeetingSpecification>().As<IShiftTradeSpecification>();

			builder.RegisterType<ShiftTradeValidator>().As<IShiftTradeValidator>();
			builder.RegisterType<ShiftTradeLightValidator>().As<IShiftTradeLightValidator>();

			builder.RegisterType<ShiftTradeRequestSetChecksum>().As<IShiftTradeRequestSetChecksum>().SingleInstance();
			builder.RegisterType<ShiftTradeRequestStatusChecker>().As<IShiftTradeRequestStatusChecker>().SingleInstance();
			builder.RegisterType<PersonListExtractorFromScheduleParts>().As<IPersonListExtractorFromScheduleParts>().SingleInstance();
			builder.RegisterType<PeriodExctractorFromScheduleParts>().As<PeriodExctractorFromScheduleParts>().SingleInstance();
			builder.RegisterType<MatrixListFactory>().As<IMatrixListFactory>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixUserLockLocker>().As<IMatrixUserLockLocker>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixNotPermittedLocker>().As<IMatrixNotPermittedLocker>().InstancePerLifetimeScope();
			builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>().SingleInstance();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>();
			builder.RegisterType<SchedulerStateHolder>().As<ISchedulerStateHolder>();
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>().SingleInstance();
		}
	}
}