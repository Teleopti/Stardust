﻿using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class NodeHandlersModule : Module
	{
		private readonly IocConfiguration _configuration;

		public NodeHandlersModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new RuleSetModule(_configuration, true));

			builder.RegisterType<StardustHealthCheckHandler>().As<IHandle<StardustHealthCheckEvent>>().SingleInstance();
			builder.RegisterType<RunRequestWaitlistStardustHandler>().As<IHandle<RunRequestWaitlistEvent>>().SingleInstance();
			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkillEvent>>().SingleInstance();
			builder.RegisterType<ApproveRequestsWithValidatorsEventStardustHandler>().As<IHandle<ApproveRequestsWithValidatorsEvent>>().SingleInstance();
			builder.RegisterType<ExportMultisiteSkillsToSkillEventHandler>().As<IHandle<ExportMultisiteSkillsToSkillEvent>>().SingleInstance();
			builder.RegisterType<IndexMaintenanceHandler>().As<IHandle<IndexMaintenanceEvent>>().SingleInstance();
			builder.RegisterType<RunSchedulingHandler>().As<IHandle<ScheduleOnNode>>().SingleInstance().ApplyAspects();
			builder.RegisterType<PersonAbsenceRemovedHandler>().As<IHandle<PersonAbsenceRemovedEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<RequestPersonAbsenceRemovedHandler>().As<IHandle<RequestPersonAbsenceRemovedEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<SchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>();
			builder.RegisterType<ExportPayrollHandler>().As<IHandle<RunPayrollExportEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterModule<PayrollContainerInstaller>();
			builder.RegisterType<StardustJobFeedback>().As<IStardustJobFeedback>().SingleInstance();
			builder.RegisterType<UpdateStaffingLevelReadModelHandler>().As<IHandle<UpdateStaffingLevelReadModelEvent>>().SingleInstance();
			builder.RegisterType<ValidateReadModelsHandler>().As<IHandle<ValidateReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<FixReadModelsHandler>().As<IHandle<FixReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsHandler>().As<IHandle<NewMultiAbsenceRequestsCreatedEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsUpdater>().As<IMultiAbsenceRequestsUpdater>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebScheduleHandler>().As<IHandle<WebScheduleStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebDayOffOptimizationHandler>().As<IHandle<WebDayoffOptimizationStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebIntradayOptimizationHandler>().As<IHandle<WebIntradayOptimizationStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
		}
	}
}