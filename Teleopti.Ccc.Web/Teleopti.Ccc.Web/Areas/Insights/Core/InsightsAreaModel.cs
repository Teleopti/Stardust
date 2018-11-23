using Autofac;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Insights.Core
{
	public class InsightsAreaModel : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PowerBiClientFactory>().As<IPowerBiClientFactory>();
			builder.RegisterType<ReportProvider>().As<IReportProvider>();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>();
		}
	}
}