namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Interface for items that needs to get the focused set
    /// </summary>
    /// <remarks>
    /// I think we should try to avoid setting the focus as much as we can
    /// Created by: henrika
    /// Created date: 2010-06-17
    /// </remarks>
    public interface IFocusTarget
    {
        /// <summary>
        /// Sets the focus on the target
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-17
        /// </remarks>
        void SetFocus();
    }
}