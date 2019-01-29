using System;
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
			
			//normally register handlers like this (=will refresh toggles)
			builder.RegisterType<ImportForecastFromFileHandler>().Named<IHandle<ImportForecastsFileToSkillEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<ApproveRequestsWithValidatorsEventStardustHandler>().Named<IHandle<ApproveRequestsWithValidatorsEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<ExportMultisiteSkillsToSkillEventHandler>().Named<IHandle<ExportMultisiteSkillsToSkillEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<IndexMaintenanceHandler>().Named<IHandle<IndexMaintenanceEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<PersonAbsenceRemovedHandler>().Named<IHandle<PersonAbsenceRemovedEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<ExportPayrollHandler>().Named<IHandle<RunPayrollExportEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<UpdateStaffingLevelReadModelHandler>().Named<IHandle<UpdateStaffingLevelReadModelEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<ValidateReadModelsHandler>().Named<IHandle<ValidateReadModelsEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<FixReadModelsHandler>().Named<IHandle<FixReadModelsEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsHandler>().Named<IHandle<NewMultiAbsenceRequestsCreatedEvent>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebScheduleHandler>().Named<IHandle<WebScheduleStardustEvent>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebIntradayOptimizationHandler>().Named<IHandle<IntradayOptimizationOnStardustWasOrdered>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebClearScheduleHandler>().Named<IHandle<WebClearScheduleStardustEvent>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ImportAgentHandler>().Named<IHandle<ImportAgentEvent>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ImportExternalPerformanceHandler>().Named<IHandle<ImportExternalPerformanceInfoEvent>>(NodeComponentName).InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ProcessWaitlistedRequestsHandler>().Named<IHandle<ProcessWaitlistedRequestsEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<RecalculateBadgeHandler>().Named<IHandle<RecalculateBadgeEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<RefreshPayrollFormatsHandler>().Named<IHandle<RefreshPayrollFormatsEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<IntradayToolHandler>().Named<IHandle<IntradayToolEvent>>(NodeComponentName).SingleInstance();
			builder.RegisterType<ImportScheduleNodeHandler>().Named<IHandle<ImportScheduleEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<CopyScheduleNodeHandler>().Named<IHandle<CopyScheduleEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			builder.RegisterType<SkillForecastReadModelHandler>().Named<IHandle<ForecastChangedEvent>>(NodeComponentName).SingleInstance().ApplyAspects();
			
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