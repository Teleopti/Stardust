using Autofac;
using Teleopti.Ccc.Web.Areas.Reports.Core;

namespace Teleopti.Ccc.Web.Areas.Reports.IoC
{
	public class ReportsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReportNavigationProvider>().As<IReportNavigationProvider>().SingleInstance();
		}
	}
}