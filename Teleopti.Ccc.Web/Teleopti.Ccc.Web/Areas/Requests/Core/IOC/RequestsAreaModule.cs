﻿using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.IOC
{
	public class RequestsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RequestsProvider>().As<IRequestsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RequestFilterCreator>().As<IRequestFilterCreator>().InstancePerLifetimeScope();
			builder.RegisterType<RequestsViewModelFactory>().As<IRequestsViewModelFactory>().SingleInstance();
			builder.RegisterType<ShiftTradeRequestViewModelFactory>().As<IShiftTradeRequestViewModelFactory>().SingleInstance();
			builder.RegisterType<RequestCommandHandlingProvider>().As<IRequestCommandHandlingProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RequestViewModelMapper>().As<IRequestViewModelMapper>().InstancePerLifetimeScope();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>().InstancePerDependency();
			builder.RegisterType<SwapService>().As<ISwapService>().InstancePerDependency();
			builder.RegisterType<ResourceCalculationOnlyScheduleDayChangeCallback>().As<IScheduleDayChangeCallback>().InstancePerDependency();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>().InstancePerDependency();

		}
	}
}