using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.WinCode.Budgeting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
{
	public class BudgetModule : Module
	{
		private readonly IocConfiguration _configuration;

		public BudgetModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			groupDayWiring(builder);
			groupWeekWiring(builder);
			groupMonthWiring(builder);
			groupMain(builder);
			groupNavigatorWiring(builder);
			commandWiring(builder);
			providerWiring(builder);
			budgetGlobalWiring(builder);
            exceptionHandlerWiring(builder);
		}

	    private static void exceptionHandlerWiring(ContainerBuilder builder)
	    {
	        builder.RegisterType<GracefulDataSourceExceptionHandler>().As<IGracefulDataSourceExceptionHandler>();
	    }

	    private static void budgetGlobalWiring(ContainerBuilder builder)
		{
			builder.RegisterType<VisibleBudgetDays>().As<IVisibleBudgetDays>();
			builder.Register(c => c.Resolve<IBudgetSettingsProvider>().BudgetSettings).As<IBudgetSettings>();
		}

		private static void commandWiring(ContainerBuilder builder)
		{
			builder.RegisterType<LoadForecastedHoursCommand>().As<ILoadForecastedHoursCommand>();
			builder.RegisterType<LoadStaffEmployedCommand>().As<ILoadStaffEmployedCommand>();
		}

		private void providerWiring(ContainerBuilder builder)
		{
			builder.RegisterType<SkillDayLoadHelper>().As<SkillDayLoadHelper>();
			
			builder.RegisterType<BudgetingSkillStaffPeriodProvider>().As<IBudgetSkillStaffPeriodProvider>();
			builder.RegisterType<BudgetPeopleProvider>().As<IBudgetPeopleProvider>();
			builder.RegisterType<BudgetSettingsProvider>().SingleInstance().As<IBudgetSettingsProvider>();
			builder.RegisterType<BudgetGroupDataService>().As<IBudgetGroupDataService>();
		    builder.RegisterType<BudgetPermissionService>().SingleInstance().As<IBudgetPermissionService>();
		}

		private static void groupNavigatorWiring(ContainerBuilder builder)
		{
			builder.RegisterType<BudgetGroupNavigatorDataService>().As<IBudgetNavigatorDataService>();
			builder.RegisterType<BudgetGroupNavigatorPresenter>().SingleInstance();
			builder.RegisterType<BudgetGroupGroupNavigatorView>()
				.As<IBudgetGroupNavigatorView>()
				.As<BudgetGroupGroupNavigatorView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupNavigatorPresenter>())
				.SingleInstance();
		}

		private static void groupMain(ContainerBuilder builder)
		{
			builder.RegisterType<BudgetGroupMainView>()
				.As<IBudgetGroupMainView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupMainPresenter>())
				.InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupMainModel>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupMainPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<HandleBudgetScope>().As<IBudgetGroupMainViewFactory>();
		    builder.RegisterType<BudgetGroupTabPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupTabView>()
				.As<IBudgetGroupTabView>()
                .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupTabPresenter>())
				.As<ISelectedBudgetDays>()
				.As<IBudgetDaySource>()
				.InstancePerLifetimeScope();
		}

		private static void groupMonthWiring(ContainerBuilder builder)
		{
			builder.RegisterType<BudgetGroupMonthView>()
				.As<IBudgetGroupMonthView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupMonthPresenter>())
				.InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupMonthModel>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupMonthPresenter>().InstancePerLifetimeScope();
		}

		private static void groupWeekWiring(ContainerBuilder builder)
		{
			builder.RegisterType<BudgetGroupWeekView>()
				.As<IBudgetGroupWeekView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupWeekPresenter>())
				.InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupWeekModel>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupWeekPresenter>().InstancePerLifetimeScope();
		}

		private static void groupDayWiring(ContainerBuilder builder)
		{
			builder.RegisterType<BudgetGroupDayPresenter>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupDayView>()
				.As<IBudgetGroupDayView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<BudgetGroupDayPresenter>())
				.InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupDayModel>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetDayReassociator>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetDayProvider>().As<IBudgetDayProvider>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetDayGapFiller>().As<IBudgetDayGapFiller>();
		}

		private class HandleBudgetScope : IBudgetGroupMainViewFactory
		{
			private readonly IComponentContext _container;
			private IDictionary<IBudgetGroupMainView, ILifetimeScope> innerScopes;

			public HandleBudgetScope(IComponentContext container)
			{
				_container = container;
				innerScopes = new Dictionary<IBudgetGroupMainView, ILifetimeScope>();
			}

			public IBudgetGroupMainView Create(IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod, IScenario scenario)
			{
				var lifetimeScope = _container.Resolve<ILifetimeScope>();
				var inner = lifetimeScope.BeginLifetimeScope();

				var budgetGroupMainModel = inner.Resolve<BudgetGroupMainModel>();
				budgetGroupMainModel.BudgetGroup =budgetGroup;
				budgetGroupMainModel.Period = dateOnlyPeriod;
				budgetGroupMainModel.Scenario = scenario;
				var budgetMain = inner.Resolve<IBudgetGroupMainView>();
			    var form = (Form)budgetMain;
				//correct event?
				form.FormClosed += mainFormClosed;
				innerScopes[budgetMain] = inner;
				return budgetMain;
			}

			private void mainFormClosed(object sender, FormClosedEventArgs e)
			{
				var form = (IBudgetGroupMainView) sender;
				innerScopes[form].Dispose();
			    innerScopes.Remove(form);
			}
		}
	}
}
