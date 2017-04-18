using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ImportForecast;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.JobHistory;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ServiceBus;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting
{
    public class ForecasterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            jobHistoryView(builder);
            importForecastView(builder);
        }

        private static  void importForecastView(ContainerBuilder builder)
        {
            builder.RegisterType<SaveImportForecastFileCommand>().As<ISaveImportForecastFileCommand>();
            builder.RegisterType<ValidateImportForecastFileCommand>().As<IValidateImportForecastFileCommand>();

			if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["FreemiumForecast"]))
				builder.RegisterType<ServiceBusSender>()
				 .As<IServiceBusSender>()
				 .SingleInstance();
			else
				builder.RegisterType<NoServiceBusSender>()
				 .As<IServiceBusSender>()
				 .SingleInstance();
            builder.RegisterType<ImportForecastView>()
                .As<IImportForecastView>()
                .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<ImportForecastPresenter>())
                .InstancePerLifetimeScope();
            builder.RegisterType<HandleImportForecastScope>().As<IImportForecastViewFactory>();
            builder.RegisterType<ImportForecastPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ImportForecastModel>().InstancePerLifetimeScope();
            
        }

        private static void jobHistoryView(ContainerBuilder builder)
        {
            builder.RegisterType<JobResultProvider>().As<IJobResultProvider>();
            builder.Register(c => new PagingDetail {Take = 20});
            builder.RegisterType<JobHistoryDetailedView>()
                .As<IJobHistoryView>()
                .OnActivated(e => e.Instance.Presenter = e.Context.Resolve<JobHistoryPresenter>())
                .InstancePerLifetimeScope();
            builder.RegisterType<HandleJobHistoryScope>().As<IJobHistoryViewFactory>();
            builder.RegisterType<JobHistoryPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ForecastsRowExtractor>().As<IForecastsRowExtractor>();
        }

        private class HandleJobHistoryScope : IJobHistoryViewFactory
        {
            private readonly IComponentContext _container;
            private readonly IDictionary<IJobHistoryView, ILifetimeScope> innerScopes;

            public HandleJobHistoryScope(IComponentContext container)
            {
                _container = container;
                innerScopes = new Dictionary<IJobHistoryView, ILifetimeScope>();
            }

            public IJobHistoryView Create()
            {
                var lifetimeScope = _container.Resolve<ILifetimeScope>();
                var inner = lifetimeScope.BeginLifetimeScope();

                var jobHistoryView = inner.Resolve<IJobHistoryView>();
                var form = (Form)jobHistoryView;
                form.ShowDialog();
                //correct event?
                form.FormClosed += mainFormClosed;
                innerScopes[jobHistoryView] = inner;
                return jobHistoryView;
            }

            private void mainFormClosed(object sender, FormClosedEventArgs e)
            {
                var form = (IJobHistoryView)sender;
                innerScopes[form].Dispose();
                innerScopes.Remove(form);
            }
        }

        private class HandleImportForecastScope : IImportForecastViewFactory
        {
            private readonly IComponentContext _container;
            private readonly IDictionary<IImportForecastView, ILifetimeScope> innerScopes;

            public HandleImportForecastScope(IComponentContext container)
            {
                _container = container;
                innerScopes = new Dictionary<IImportForecastView, ILifetimeScope>();
            }

            public IImportForecastView Create(ISkill skill)
            {
                var lifetimeScope = _container.Resolve<ILifetimeScope>();
                var inner = lifetimeScope.BeginLifetimeScope();

                var model = inner.Resolve<ImportForecastModel>();
                model.SelectedSkill = skill;

                var view = inner.Resolve<IImportForecastView>();
                var form = (Form)view;
                form.ShowDialog();
                //correct event?
                form.FormClosed += mainFormClosed;
                innerScopes[view] = inner;
                return view;
            }

            private void mainFormClosed(object sender, FormClosedEventArgs e)
            {
                var form = (IImportForecastView)sender;
                innerScopes[form].Dispose();
                innerScopes.Remove(form);
            }
        }
    }

	

	public interface IImportForecastViewFactory
    {
        IImportForecastView Create(ISkill skill);
    }
}