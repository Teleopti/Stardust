using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReportModule : Module
	{
		private readonly IocConfiguration _configuration;

		public ReportModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			if (!_configuration.IsToggleEnabled(Toggles.Report_Remove_Realtime_Scheduled_Time_vs_Target_45559))
				builder.RegisterType<ReportScheduledTimeVsTargetVisible>().As<IReportVisible>().SingleInstance();

			builder.RegisterType<ReportNavigationModel>().As<IReportNavigationModel>();

			builder.RegisterType<PersonsWhoChangedSchedulesViewModelProvider>().SingleInstance();
			builder.RegisterType<ScheduleAuditTrailReport>().As<IScheduleAuditTrailReport>().SingleInstance();
			builder.RegisterType<ScheduleAuditTrailReportViewModelProvider>().SingleInstance();
			builder.RegisterType<ScheduleAnalysisAuditTrailProvider>().As<IScheduleAnalysisProvider>().SingleInstance();
		}
	}
}