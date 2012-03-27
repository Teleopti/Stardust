using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Forecasting.Forms.JobHistory;
using Teleopti.Ccc.WinCode.Forecasting;

namespace Teleopti.Ccc.Win.Forecasting
{
    public class ForecasterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            jobHistoryView(builder);
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
    }
}