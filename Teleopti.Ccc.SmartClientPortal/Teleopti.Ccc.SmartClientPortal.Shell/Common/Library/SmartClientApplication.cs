using System.Runtime;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.WPF;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Library
{
    public abstract class SmartClientApplication<TWorkItem, TShell> : WPFFormShellApplication<TWorkItem, TShell>
        where TWorkItem : WorkItem, new()
        where TShell : Form
    {
        private readonly IComponentContext _container;

        protected SmartClientApplication(IComponentContext container)
        {
			ProfileOptimization.StartProfile("SmartClientApplication.Profile");
            _container = container;
        }

        protected override void AddServices()
        {
            base.AddServices();
			
            RootWorkItem.Services.Add(_container);
        }
    }
}

