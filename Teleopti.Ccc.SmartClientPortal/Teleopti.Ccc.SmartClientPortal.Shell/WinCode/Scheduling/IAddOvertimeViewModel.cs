using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    /// <summary>
    /// Add overtime
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-31
    /// </remarks>
    public interface IAddOvertimeViewModel :  IAddLayerViewModel<IActivity>
    {
        /// <summary>
        /// Gets the selected multiplicator definition set.
        /// </summary>
        /// <value>The selected multiplicator definition set.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-08-31
        /// </remarks>
        IMultiplicatorDefinitionSet SelectedMultiplicatorDefinitionSet { get; }
    }
}