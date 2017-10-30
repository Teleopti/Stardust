using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReportModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public ReportModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			if (!_configuration.Toggle(Toggles.Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560))
				builder.RegisterType<ReportScheduledTimePerActivityVisible>().As<IReportVisible>().SingleInstance();
			if (!_configuration.Toggle(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559))
				builder.RegisterType<ReportScheduledTimeVsTargetVisible>().As<IReportVisible>().SingleInstance();
			if (!_configuration.Toggle(Toggles.Report_Remove_Realtime_AuditTrail_44006))
				builder.RegisterType<ReportAuditTrailVisible>().As<IReportVisible>().SingleInstance();

			builder.RegisterType<ReportNavigationModel>().As<IReportNavigationModel>();

			builder.RegisterType<PersonsWhoChangedSchedulesViewModelProvider>().SingleInstance();
			builder.RegisterType<ScheduleAuditTrailReport>().As<IScheduleAuditTrailReport>().SingleInstance();
			builder.RegisterType<ScheduleAuditTrailReportViewModelProvider>().SingleInstance();

			if (_configuration.Toggle(Toggles.WFM_AuditTrail_44006))
				builder.RegisterType<ScheduleAnalysisAuditTrailProvider>().As<IScheduleAnalysisProvider>().SingleInstance();
			else
				builder.RegisterType<ScheduleAnalysisProvider>().As<IScheduleAnalysisProvider>().SingleInstance();
		}
	}
}