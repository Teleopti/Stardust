using Autofac;
using Teleopti.Ccc.Win.Budgeting;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.Win.Intraday;
using Teleopti.Ccc.Win.Matrix;
using Teleopti.Ccc.Win.Payroll;
using Teleopti.Ccc.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.Win.PerformanceManager;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.Win.Shifts;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Matrix;

namespace Teleopti.Ccc.Win.Main
{
    public class NavigationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MatrixNavigationModel>().As<IMatrixNavigationModel>();
            builder.RegisterType<MatrixNavigationView>().SingleInstance();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<NavigationPanelProvider>().SingleInstance();
            builder.RegisterType<ForecasterNavigator>();
            //builder.RegisterType<IntradayNavigationPanel>();
            builder.RegisterType<IntradayNavigator>();
            //builder.RegisterType<SchedulerNavigationPanel>();
            builder.RegisterType<PortalSettingsProvider>().SingleInstance();
            builder.RegisterType<BudgetGroupGroupNavigatorView>();
            builder.RegisterType<BudgetGroupNavigatorModel>();
            builder.RegisterType<PerformanceManagerNavigator>();
            //builder.RegisterType<PeopleAdminNavigationPanel>();
            builder.RegisterType<PayrollExportNavigator>();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<PeopleNavigator>();
            builder.RegisterType<SchedulerNavigator>();
            builder.Register(c => c.Resolve<PortalSettingsProvider>().PortalSettings).As<PortalSettings>().As<IPortalSettings>();
        }
    }
}
