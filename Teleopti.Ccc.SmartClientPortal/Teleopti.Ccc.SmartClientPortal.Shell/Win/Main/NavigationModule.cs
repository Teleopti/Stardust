using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Matrix;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PerformanceManager;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Matrix;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
    public class NavigationModule : Module
    {
	    private readonly IIocConfiguration _config;

	    public NavigationModule(IIocConfiguration config)
	    {
		    _config = config;
	    }

	    protected override void Load(ContainerBuilder builder)
		 {
			 builder.RegisterType<ReportNavigationModel>().As<IReportNavigationModel>();


		    builder.Register(c => new ReportUrlConstructor(_config.Args().ReportServer, c.Resolve<IConfigReader>()))
			    .As<IReportUrl>()
			    .SingleInstance();

				builder.RegisterType<MatrixNavigationView>().SingleInstance();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<NavigationPanelProvider>().SingleInstance();
            builder.RegisterType<ForecasterNavigator>();
            builder.RegisterType<IntradayNavigator>();
            builder.RegisterType<IntradayWebNavigator>();
            builder.RegisterType<PortalSettingsProvider>().SingleInstance();
            builder.RegisterType<BudgetGroupGroupNavigatorView>();
            builder.RegisterType<BudgetGroupNavigatorModel>();
            builder.Register(c => new PerformanceManagerNavigator(_config.Args().MatrixWebSiteUrl));
            builder.RegisterType<PayrollExportNavigator>();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<PeopleNavigator>();
            builder.RegisterType<SchedulerNavigator>();
            builder.Register(c => c.Resolve<PortalSettingsProvider>().PortalSettings).As<PortalSettings>().As<IPortalSettings>();
        }
    }
}
