using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Reports;

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
			builder.RegisterType<ReportAuditTrailVisible>().As<IReportVisible>().SingleInstance();

			builder.RegisterType<ScheduleChangedByUserViewModelProvider>().SingleInstance();
		}
	}
}