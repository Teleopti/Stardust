using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Permissions
{
    public interface IPermissionViewerRoles
    {
        Guid SelectedPerson { get; }
        Guid SelectedFunction { get; }
        Guid SelectedData { get; }
        int SelectedDataRange { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        TreeNodeAdv[] AllDataNodes { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        TreeNodeAdv[] AllFunctionNodes { get; }
        void FillPersonsMainList(ListViewItem[] listViewItems);
        void FillPersonRolesList(ListViewItem[] listViewItems);
        void FillFunctionPersonsList(ListViewItem[] listViewItems);
        void FillFunctionRolesList(ListViewItem[] listViewItems);
        void FillDataPersonsList(ListViewItem[] listViewItems);
        void FillDataRolesList(ListViewItem[] listViewItems);

        void Show();
        void BringToFront();
        void FillDataTree(TreeNodeAdv[] treeNodes, TreeNodeAdv[] dataTreeNodes, TreeNodeAdv[] allTreeNodes);
        void FillFunctionTree(TreeNodeAdv[] personFunctionNodes, TreeNodeAdv[] mainFunctionNodes, TreeNodeAdv[] allTreeNodes);
        void ShowDataSourceException(Infrastructure.Foundation.DataSourceException dataSourceException);
        void Close();
    }
}