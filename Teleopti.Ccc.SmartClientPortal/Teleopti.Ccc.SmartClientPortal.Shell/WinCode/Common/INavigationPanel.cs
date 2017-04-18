using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface INavigationPanel
    {
        void Open();
        bool OpenEnabled { get; set; }
        bool AddGroupPageEnabled { get; set; }
        bool RenameGroupPageEnabled { get; set; }
        bool DeleteGroupPageEnabled { get; set; }
        bool ModifyGroupPageEnabled { get; set; }
        Cursor Cursor { get; set; }
	    void SetMainOwner(Form mainWindow);
    }
}