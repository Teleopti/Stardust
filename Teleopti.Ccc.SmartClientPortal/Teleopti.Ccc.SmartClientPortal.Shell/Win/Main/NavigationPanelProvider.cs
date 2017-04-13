using System.Windows.Forms;
using Autofac;

namespace Teleopti.Ccc.Win.Main
{
    public class NavigationPanelProvider
    {
        private readonly IComponentContext _componentContext;

        public NavigationPanelProvider(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public T CreateNavigationPanel<T>() where T : UserControl
        {
            T navigationPanel = _componentContext.Resolve<T>();
            navigationPanel.Dock = DockStyle.Fill;
            return navigationPanel;
        }
    }
}
