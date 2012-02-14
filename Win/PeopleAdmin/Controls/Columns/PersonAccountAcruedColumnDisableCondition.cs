using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    /// <summary>
    /// Represents the column disable condition for the column accrued of the person account.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-11-21
    /// </remarks>
    public class PersonAccountParentViewAccruedColumnDisableCondition : IColumnDisableCondition<IPersonAccountModel>
    {
        public bool IsColumnDisable(IPersonAccountModel dataItem, string bindingProperty)
        {
            bool isColumnDisabled = false;

            // If the tracker is a comp tracker and the column is accrued we need to disable the cell.
            if ((dataItem.CurrentAccount.Owner.Absence.Tracker == Tracker.CreateCompTracker()) && (bindingProperty == "Accrued"))
            {
                isColumnDisabled = true;
            }

            return isColumnDisabled;
        }
    }

    public class PersonAccountChildViewAccruedColumnDisableCondition : IColumnDisableCondition<IPersonAccountChildModel>
    {
        public bool IsColumnDisable(IPersonAccountChildModel dataItem, string bindingProperty)
        {
            bool isColumnDisabled = false;

            // If the tracker is a comp tracker and the column is accrued we need to disable the cell.
            if ((dataItem.ContainedEntity.Owner.Absence.Tracker == Tracker.CreateCompTracker()) && (bindingProperty == "Accrued"))
            {
                isColumnDisabled = true;
            }

            return isColumnDisabled;
        }
    }
}
