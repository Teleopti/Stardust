using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPreferenceView
    {
        void CellDataLoaded();
        void SelectColumns(int left, int right, int top, int bottom);
        void SelectRows(int left, int right, int top, int bottom);
        void SelectAll(int left, int right, int top, int bottom);
        PreferencePresenter Presenter { get; }
        void AddShiftCategoryToContextMenu(ShiftCategory shiftCategory);
        void AddAbsenceToContextMenu(Absence absence);
        void AddDayOffToContextMenu(DayOff dayOff);
        void SetupContextMenu();
        void SetValidationInfoText(string text, Color color, string dayOffsText, Color dayOffsColor, string calculationInfo);
        void SetValidationPicture(Bitmap picture);
        void ClearContextMenus();
        void ToggleStateContextMenuItemPaste(bool enable);
        void RefreshExtendedPreference();
        void SetDaysOff(IList<DayOff> daysOff);
        void SetShiftCategories(IList<ShiftCategory> shiftCategories);
        void SetAbsences(IList<Absence> absences);
        void ToggleStateContextMenuItemSaveAsTemplate(bool enabled);
        void ShowErrorItemNoLongerAvailable(string itemName);
    }
}