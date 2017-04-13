using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Autofac;
using Autofac.Core;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Ccc.WinCode.Shifts.Presenters;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Module = Autofac.Module;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public class ShiftsModule : Module
    {
        public interface IShiftsExplorerFactory
        {
            IExplorerView Create(Form mainWindow);
        }

        protected override void Load(ContainerBuilder builder)
        {
            shiftsMain(builder);
        }

        private static void shiftsMain(ContainerBuilder builder)
        {
            builder.RegisterType<ShiftsDataHelper>().As<IDataHelper>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftsExplorerFactory>().As<IShiftsExplorerFactory>();
            builder.RegisterType<ExplorerPresenter>().As<IExplorerPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ExplorerViewModel>().As<IExplorerViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftsExplorer>().As<IExplorerView>().InstancePerLifetimeScope();

            //builder.RegisterType<NavigationPresenter>().As<INavigationPresenter>().InstancePerLifetimeScope();
        }



        private class ShiftsExplorerFactory : IShiftsExplorerFactory
        {
            private readonly IComponentContext _container;
            private readonly IDictionary<IExplorerView, ILifetimeScope> _innerScopes;

	        public ShiftsExplorerFactory(IComponentContext container)
            {
                _container = container;
                _innerScopes = new Dictionary<IExplorerView, ILifetimeScope>();
            }

            public IExplorerView Create(Form mainWindow)
            {
                try
                {
                    var lifetimeScope = _container.Resolve<ILifetimeScope>();
                    var inner = lifetimeScope.BeginLifetimeScope();
                    var explorerPresenter = inner.Resolve<IExplorerPresenter>();
					explorerPresenter.Show(mainWindow);
                    var shiftsView = inner.Resolve<IExplorerView>();
                    var form = (Form)shiftsView;
                    //correct event?
                    form.FormClosed += mainFormClosed;
                    _innerScopes[shiftsView] = inner;
                    return shiftsView;
                }
                catch(DependencyResolutionException exception)
                {
                    if (exception.InnerException != null && exception.InnerException is DataSourceException)
                        throw exception.InnerException;
                    throw;
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null && exception.InnerException is DataSourceException)
                        throw exception.InnerException;
                    throw;
                }
            }

            private void mainFormClosed(object sender, FormClosedEventArgs e)
            {
                var form = (Form)sender;
                form.FormClosed -= mainFormClosed;
                var overview = (IExplorerView)sender;
                _innerScopes[overview].Dispose();
                _innerScopes.Remove(overview);
            }
        }
    }
}