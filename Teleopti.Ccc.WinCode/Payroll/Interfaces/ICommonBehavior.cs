
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
        /// Renames this instance.
        /// </summary>
        void Rename();

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        void Sort(SortingMode mode);

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        void Cut();

        /// <summary>
        /// Copies this instance.
        /// </summary>
        void Copy();

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        void Paste();

        /// <summary>
        /// Moves up.
        /// </summary>
        void MoveUp();

        /// <summary>
        /// Moves down.
        /// </summary>
        void MoveDown();

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
