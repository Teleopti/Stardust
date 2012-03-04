using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Permissions
{
    public interface IPermissionViewerRoles
    {
        Guid SelectedPerson { get; }
        Guid SelectedFunction { get; }
        Guid SelectedRole { get; }
        void FillPersonsMainList(ListViewItem[] listViewItems);
        void FillPersonRolesList(ListViewItem[] listViewItems);
        void FillPersonFunctionsList(ListViewItem[] listViewItems);
        void FillFunctionsMainList(ListViewItem[] listViewItems);
        void FillFunctionPersonsList(ListViewItem[] listViewItems);
        void FillFunctionRolesList(ListViewItem[] listViewItems);
        void FillRolesMainList(ListViewItem[] listViewItems);

        void Show();
    }
}