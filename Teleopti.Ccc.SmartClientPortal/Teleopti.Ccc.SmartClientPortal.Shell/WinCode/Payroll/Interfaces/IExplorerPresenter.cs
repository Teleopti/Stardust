
namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// Explorer presenter
    /// </summary>
    public interface IExplorerPresenter : IPresenterBase
    {
        IExplorerViewModel Model { get; }

        /// <summary>
        /// Gets the definition set presenter.
        /// </summary>
        /// <value>The definition set presenter.</value>
    
        IDefinitionSetPresenter DefinitionSetPresenter { get; }

        IMultiplicatorDefinitionPresenter MultiplicatorDefinitionPresenter { get; }

        /// <summary>
        /// Gets the visualize presenter.
        /// </summary>
        /// <value>The visualize presenter.</value>
        IVisualizePresenter VisualizePresenter { get; }
    }
}
