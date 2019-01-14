﻿using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class NodeHandlersModule : Module
	{
		private readonly IocConfiguration _configuration;

		public NodeHandlersModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		public static string NodeComponentName { get; } = "StardustHandler";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>();
			builder.RegisterModule(new PayrollContainerInstaller(_configuration));
			builder.RegisterType<StardustJobFeedback>().As<IStardustJobFeedback>().SingleInstance();
			builder.RegisterType<MultiAbsenceRequestsUpdater>().As<IMultiAbsenceRequestsUpdater>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<TenantAuthenticationAlwaysAuthenticated>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<ScheduleDayDifferenceSaver>().As<IScheduleDayDifferenceSaver>().SingleInstance();
			builder.RegisterType<PayrollFormatRepositoryFactory>().As<IPayrollFormatRepositoryFactory>().SingleInstance();
			
			builder.RegisterType<StardustHealthCheckHandler>().As<IHandle<StardustHealthCheckEvent>>().SingleInstance();
			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkillEvent>>().SingleInstance();
			builder.RegisterType<ApproveRequestsWithValidatorsEventStardustHandler>().As<IHandle<ApproveRequestsWithValidatorsEvent>>().SingleInstance();
			builder.RegisterType<ExportMultisiteSkillsToSkillEventHandler>().As<IHandle<ExportMultisiteSkillsToSkillEvent>>().SingleInstance();
			builder.RegisterType<IndexMaintenanceHandler>().As<IHandle<IndexMaintenanceEvent>>().SingleInstance();
			builder.RegisterType<PersonAbsenceRemovedHandler>().As<IHandle<PersonAbsenceRemovedEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<ExportPayrollHandler>().As<IHandle<RunPayrollExportEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<UpdateStaffingLevelReadModelHandler>().As<IHandle<UpdateStaffingLevelReadModelEvent>>().SingleInstance();
			builder.RegisterType<ValidateReadModelsHandler>().As<IHandle<ValidateReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<FixReadModelsHandler>().As<IHandle<FixReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsHandler>().As<IHandle<NewMultiAbsenceRequestsCreatedEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebScheduleHandler>().As<IHandle<WebScheduleStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebIntradayOptimizationHandler>().As<IHandle<IntradayOptimizationOnStardustWasOrdered>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebClearScheduleHandler>().As<IHandle<WebClearScheduleStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ImportAgentHandler>().As<IHandle<ImportAgentEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ImportExternalPerformanceHandler>().As<IHandle<ImportExternalPerformanceInfoEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ProcessWaitlistedRequestsHandler>().As<IHandle<ProcessWaitlistedRequestsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<RecalculateBadgeHandler>().As<IHandle<RecalculateBadgeEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<RefreshPayrollFormatsHandler>().As<IHandle<RefreshPayrollFormatsEvent>>().SingleInstance();
			builder.RegisterType<IntradayToolHandler>().As<IHandle<IntradayToolEvent>>().SingleInstance();
			builder.RegisterType<ImportScheduleNodeHandler>().As<IHandle<ImportScheduleEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<CopyScheduleNodeHandler>().As<IHandle<CopyScheduleEvent>>().SingleInstance().ApplyAspects();
			
			builder.RegisterGenericDecorator(typeof(refreshTogglesDecorator<>), typeof(IHandle<>), NodeComponentName);
		}

		private class refreshTogglesDecorator<T> : IHandle<T>
		{
			private readonly IHandle<T> _inner;
			private readonly IToggleFiller _toggleFiller;

			public refreshTogglesDecorator(IHandle<T> inner, IToggleFiller toggleFiller)
			{
				_inner = inner;
				_toggleFiller = toggleFiller;
			}
			
			public void Handle(T parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
				ref IEnumerable<object> returnObjects)
			{
				_toggleFiller.RefetchToggles();
				_inner.Handle(parameters, cancellationTokenSource, sendProgress, ref returnObjects);
			}
		}
	}
}