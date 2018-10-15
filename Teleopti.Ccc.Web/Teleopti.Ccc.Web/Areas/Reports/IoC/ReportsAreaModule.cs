using Autofac;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;

namespace Teleopti.Ccc.Web.Areas.Reports.IoC
{
	public class ReportsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReportNavigationProvider>().As<IReportNavigationProvider>().SingleInstance();
			builder.RegisterType<AgentBadgeProvider>().As<IAgentBadgeProvider>().SingleInstance();
			builder.RegisterType<AuditService>().As<IAuditService>().SingleInstance();
		}
	}
}