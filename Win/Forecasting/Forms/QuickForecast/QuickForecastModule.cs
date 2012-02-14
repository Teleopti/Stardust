using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public class QuickForecastModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			groupMain(builder);
			commandWiring(builder);
			providerWiring(builder);
			
			//denna ska inte vara här, borde inte vara här igen
			builder.RegisterType<OutlierRepository>().As<IOutlierRepository>();
			builder.RegisterType<SkillDayRepository>().As<ISkillDayRepository>();
			builder.RegisterType<WorkloadRepository>().As<IWorkloadRepository>();
			builder.RegisterType<ScenarioRepository>().As<IScenarioRepository>();
		}

		private static void commandWiring(ContainerBuilder builder)
		{
			builder.RegisterType<QuickForecastCommand>();
        }

		private static void providerWiring(ContainerBuilder builder)
		{
			builder.RegisterType<QuickForecastScenarioProvider>().As<IQuickForecastScenarioProvider>();
			builder.RegisterType<WorkloadProvider>().As<IWorkloadProvider>();
		    builder.RegisterType<QuickForecastPeriodProvider>().As<IQuickForecastPeriodProvider>();
		}

		private static void groupMain(ContainerBuilder builder)
		{
			builder.RegisterType<QuickForecastView>()
				.As<IQuickForecastView>()
				.OnActivated(e => e.Instance.Presenter = e.Context.Resolve<QuickForecastPresenter>())
				.InstancePerLifetimeScope();
			builder.RegisterType<QuickForecastModel>().InstancePerLifetimeScope();
			builder.RegisterType<QuickForecastPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<HandleQuickForecastScope>().As<IQuickForecastViewFactory>();
		}

		private class HandleQuickForecastScope : IQuickForecastViewFactory
		{
			private readonly IComponentContext _container;
			private IDictionary<IQuickForecastView, ILifetimeScope> innerScopes;

            public HandleQuickForecastScope(IComponentContext container)
			{
				_container = container;
                innerScopes = new Dictionary<IQuickForecastView, ILifetimeScope>();
			}

            public IQuickForecastView Create()
			{
				var lifetimeScope = _container.Resolve<ILifetimeScope>();
				var inner = lifetimeScope.BeginLifetimeScope();

                var autoForecastView = inner.Resolve<IQuickForecastView>();
			    var form = (Form)autoForecastView;
				form.Show();
				//correct event?
				form.FormClosed += mainFormClosed;
				innerScopes[autoForecastView] = inner;
				return autoForecastView;
			}

			private void mainFormClosed(object sender, FormClosedEventArgs e)
			{
                var form = (IQuickForecastView)sender;
				innerScopes[form].Dispose();
			    innerScopes.Remove(form);
			}
		}
	}

    public interface IQuickForecastViewFactory
    {
        IQuickForecastView Create();
    }
}
