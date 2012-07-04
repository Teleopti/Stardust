using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC
{
	using Providers;

	public class MobileReportsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			registerCommonTypes(builder);
			registerReportTypes(builder);
		}

		private static void registerReportTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ReportViewModelFactory>().As<IReportViewModelFactory>();
			builder.RegisterType<DefinedReportProvider>().As<IDefinedReportProvider>().SingleInstance();
			builder.RegisterType<SkillProvider>().As<ISkillProvider>();
			builder.RegisterType<WebReportUserInfoProvider>().As<IWebReportUserInfoProvider>();
			builder.RegisterType<ReportDataService>().As<IReportDataService>();
			builder.RegisterType<ReportDataFetcher>().As<IReportRequestValidator>();
			builder.RegisterType<DateBoxGlobalizationViewModelFactory>().As<IDateBoxGlobalizationViewModelFactory>();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<UserTextTranslator>().As<IUserTextTranslator>().SingleInstance();
		}
	}
}