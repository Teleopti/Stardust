using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IExplorerView
    {
        /// <summary>
        /// Gets the width of visualize control container.
        /// </summary>
        /// <returns></returns>
        float GetWidthOfVisualizeControlContainer();

        /// <summary>
        /// Refreshes the selected views.
        /// </summary>
        void RefreshSelectedViews();

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Gets the explorer presenter.
        /// </summary>
        /// <value>The explorer presenter.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        IExplorerPresenter ExplorerPresenter { get; }
    }
}
