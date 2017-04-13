
namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// Functionality which will common to all the views in payroll module is
    /// defined here
    /// </summary>
    public interface ICommonBehavior
    {
        /// <summary>
        /// Adds the new.
        /// </summary>
        void AddNew();

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        void DeleteSelected();

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        void Reload();

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        void RefreshView();

        /// <summary>
        /// Gets delete tooltip
        /// </summary>
        string ToolTipDelete{ get; }

        /// <summary>
        /// Gets add new tooltip
        /// </summary>
        string ToolTipAddNew { get; }
    }
}
