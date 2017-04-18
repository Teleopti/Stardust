using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    /// <summary>
    /// AddActivity
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-31
    /// </remarks>
    public interface IAddActivityViewModel :IAddLayerViewModel<IActivity>
    {
        /// <summary>
        /// Gets the selected shift category.
        /// </summary>
        /// <value>The selected shift category.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-31
        /// </remarks>
        IShiftCategory SelectedShiftCategory { get; }
    }
}