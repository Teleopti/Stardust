using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Permissions
{
    public interface IPermissionViewerRoles
    {
        Guid SelectedPerson { get; }
        Guid SelectedFunction { get; }
        Guid SelectedRole { get; }
        Guid SelectedData { get; }
        int SelectedDataRange { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        TreeNodeAdv[] AllDataNodes { get; }
        void FillPersonsMainList(ListViewItem[] listViewItems);
        void FillPersonRolesList(ListViewItem[] listViewItems);
        void FillPersonFunctionsList(ListViewItem[] listViewItems);
        void FillFunctionsMainList(ListViewItem[] listViewItems);
        void FillFunctionPersonsList(ListViewItem[] listViewItems);
        void FillFunctionRolesList(ListViewItem[] listViewItems);
        void FillRolesMainList(ListViewItem[] listViewItems);
        void FillDataPersonsList(ListViewItem[] listViewItems);
        void FillDataRolesList(ListViewItem[] listViewItems);

        void Show();
        void BringToFront();
        void FillDataTree(TreeNodeAdv[] treeNodes, TreeNodeAdv[] dataTreeNodes, TreeNodeAdv[] allTreeNodes);

        void Close();
    }
}