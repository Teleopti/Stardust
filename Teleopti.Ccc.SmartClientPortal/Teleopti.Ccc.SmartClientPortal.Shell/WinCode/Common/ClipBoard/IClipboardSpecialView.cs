namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard
{
    public interface IClipboardSpecialView
    {
        void SetPermissionOnAbsences(bool permission);
        void SetPermissionOnDayOffs(bool permission);
        void SetPermissionOnPersonalAssignments(bool permission);
        void SetPermissionOnAssignments(bool permission);
        void SetPermissionOnOvertime(bool permission);
        void SetPermissionsOnRestrictions(bool permission);
	    void SetPermissionsOnShiftAsOvertime(bool permission);
        void HideForm();
        void SetTexts();
		void SetTextsNoRightToLeft();
        void SetColor();
        bool Cancel();
        void ShowRestrictions(bool show);
    }
}
