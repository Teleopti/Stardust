using System.Runtime.InteropServices;
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

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StardustHealthCheckHandler>().As<IHandle<StardustHealthCheckEvent>>().SingleInstance();
			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkillEvent>>().SingleInstance();
			builder.RegisterType<ApproveRequestsWithValidatorsEventStardustHandler>().As<IHandle<ApproveRequestsWithValidatorsEvent>>().SingleInstance();
			builder.RegisterType<ExportMultisiteSkillsToSkillEventHandler>().As<IHandle<ExportMultisiteSkillsToSkillEvent>>().SingleInstance();
			builder.RegisterType<IndexMaintenanceHandler>().As<IHandle<IndexMaintenanceEvent>>().SingleInstance();
			builder.RegisterType<PersonAbsenceRemovedHandler>().As<IHandle<PersonAbsenceRemovedEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<SchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>();
			builder.RegisterType<ExportPayrollHandler>().As<IHandle<RunPayrollExportEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterModule(new PayrollContainerInstaller(_configuration));
			builder.RegisterType<StardustJobFeedback>().As<IStardustJobFeedback>().SingleInstance();
			builder.RegisterType<UpdateStaffingLevelReadModelHandler>().As<IHandle<UpdateStaffingLevelReadModelEvent>>().SingleInstance();
			builder.RegisterType<ValidateReadModelsHandler>().As<IHandle<ValidateReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<FixReadModelsHandler>().As<IHandle<FixReadModelsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsHandler>().As<IHandle<NewMultiAbsenceRequestsCreatedEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<MultiAbsenceRequestsUpdater>().As<IMultiAbsenceRequestsUpdater>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebScheduleHandler>().As<IHandle<WebScheduleStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebIntradayOptimizationHandler>().As<IHandle<IntradayOptimizationOnStardustWasOrdered>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<WebClearScheduleHandler>().As<IHandle<WebClearScheduleStardustEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<TenantAuthenticationAlwaysAuthenticated>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<ImportAgentHandler>().As<IHandle<ImportAgentEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ImportExternalPerformanceHandler>().As<IHandle<ImportExternalPerformanceInfoEvent>>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ProcessWaitlistedRequestsHandler>().As<IHandle<ProcessWaitlistedRequestsEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<RecalculateBadgeHandler>().As<IHandle<RecalculateBadgeEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<ScheduleDayDifferenceSaver>().As<IScheduleDayDifferenceSaver>().SingleInstance();
			builder.RegisterType<RefreshPayrollFormatsHandler>().As<IHandle<RefreshPayrollFormatsEvent>>().SingleInstance();
			builder.RegisterType<IntradayToolHandler>().As<IHandle<IntradayToolEvent>>().SingleInstance();
			builder.RegisterType<ImportScheduleNodeHandler>().As<IHandle<ImportScheduleEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<CopyScheduleNodeHandler>().As<IHandle<CopyScheduleEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<PayrollFormatRepositoryFactory>().As<IPayrollFormatRepositoryFactory>().SingleInstance();
		}
	}
}