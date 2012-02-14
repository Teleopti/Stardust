using System.Drawing;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public interface IStudentAvailabilityView
    {
        void CellDataLoaded();
        void SetValidationPicture(Bitmap picture);
        void SetValidationInfoText(string text, Color color);
        void RefreshEditStudentAvailabilityView();
        void ToggleStateContextMenuItemPaste(bool enable);
        void SelectColumns(int left, int right, int top, int bottom);
        void SelectRows(int left, int right, int top, int bottom);
        void SelectAll(int left, int right, int top, int bottom);
        void SetupContextMenu();
    }
}