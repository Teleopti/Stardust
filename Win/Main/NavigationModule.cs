﻿using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
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
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
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
            builder.RegisterType<MatrixNavigationModel>().As<IMatrixNavigationModel>();
				if (_config.Toggle(Toggles.MultiTenantSSOSupport_StandardReports_15093))
					builder.RegisterType<ReportUrlConstructor>().As<IReportUrl>().SingleInstance();
				else
					builder.RegisterType<ReportUrl>().As<IReportUrl>().SingleInstance();
            builder.RegisterType<MatrixNavigationView>().SingleInstance();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<NavigationPanelProvider>().SingleInstance();
            builder.RegisterType<ForecasterNavigator>();
            builder.RegisterType<IntradayNavigator>();
            builder.RegisterType<PortalSettingsProvider>().SingleInstance();
            builder.RegisterType<BudgetGroupGroupNavigatorView>();
            builder.RegisterType<BudgetGroupNavigatorModel>();
            builder.RegisterType<PerformanceManagerNavigator>();
            builder.RegisterType<PayrollExportNavigator>();
            builder.RegisterType<ShiftsNavigationPanel>();
            builder.RegisterType<PeopleNavigator>();
            builder.RegisterType<SchedulerNavigator>();
            builder.Register(c => c.Resolve<PortalSettingsProvider>().PortalSettings).As<PortalSettings>().As<IPortalSettings>();
        }
    }
}
