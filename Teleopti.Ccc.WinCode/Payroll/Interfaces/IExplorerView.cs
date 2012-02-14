using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public enum ClipboardOperation
    {
        /// <summary>
        /// Copy
        /// </summary>
        Copy,
        /// <summary>
        /// Cut
        /// </summary>
        Cut,
        /// <summary>
        /// Paste
        /// </summary>
        Paste
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: VirajS
    /// Created date: 2009-01-20
    /// </remarks>
    public interface IExplorerView
    {
        /// <summary>
        /// Sets the selected view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        void SetSelectedView(PayrollViewType view);

        /// <summary>
        /// Checks for delete.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        bool CheckForDelete();

        /// <summary>
        /// Sets the state of the clipboard control.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        void SetClipboardControlState(ClipboardOperation action, bool status);

        /// <summary>
        /// Gets the width of visualize control container.
        /// </summary>
        /// <returns></returns>
        float GetWidthOfVisualizeControlContainer();

        /// <summary>
        /// Refreshes the specified view.
        /// </summary>
        /// <param name="?">The ?.</param>
        void Refresh(PayrollViewType view);

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

        /// <summary>
        /// Gets the clipboard action.
        /// </summary>
        /// <value>The clipboard action.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        ClipboardOperation ClipboardActionType { get; }

        
    }
}
