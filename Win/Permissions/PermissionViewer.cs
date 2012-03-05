using System;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class PermissionViewer : BaseDialogForm, IPermissionViewerRoles
    {
        private readonly IEventAggregator _eventAggregator;
        private TreeNodeAdv[] _allTreeNodeAdvs;

        public PermissionViewer(IEventAggregator eventAggregator):this()
        {
            _eventAggregator = eventAggregator;
        }

        public PermissionViewer()
        {
            InitializeComponent();
            SetTexts();
            ColorHelper.SetTabControlTheme(tabControlAdv1);
        }

        public ListView RolesMainList
        {
            get { return listViewRolesMain; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public Guid SelectedRole
        {
            get
            {
                //later
                throw new NotImplementedException();
            }
        }

        public Guid SelectedData
        {
            get
            {
                var id = treeViewDataMain.SelectedNode.TagObject;
                if (id.GetType().Equals(typeof(Guid)))
                {
                    return (Guid)id;
                }
                return Guid.Empty;
            }
        }

        public int SelectedDataRange
        {
            get
            {
                var id = treeViewDataMain.SelectedNode.TagObject;
                
                if (id.GetType().Equals(typeof(int)))
                {
                    return (int) id;
                }
                return 0;
            }
        }

        public TreeNodeAdv[] AllDataNodes
        {
            get { return _allTreeNodeAdvs; }
        }

        public void FillPersonsMainList(ListViewItem[] listViewItems)
        {
            listViewPersonsMain.Items.Clear();
            listViewPersonsMain.Items.AddRange(listViewItems);
        }

        public void FillPersonRolesList(ListViewItem[] listViewItems)
        {
            listViewPersonRoles.Items.Clear();
            listViewPersonRoles.Items.AddRange(listViewItems);
        }

        public void FillPersonFunctionsList(ListViewItem[] listViewItems)
        {
            listViewPersonFunctions.Items.Clear();
            listViewPersonFunctions.Items.AddRange(listViewItems);
        }

        public void FillFunctionsMainList(ListViewItem[] listViewItems)
        {
            listViewFunctionsMain.Items.Clear();
            listViewFunctionsMain.Items.AddRange(listViewItems);
        }

        public void FillFunctionPersonsList(ListViewItem[] listViewItems)
        {
            listViewFunctionPersons.Items.Clear();
            listViewFunctionPersons.Items.AddRange(listViewItems);
        }

        public void FillFunctionRolesList(ListViewItem[] listViewItems)
        {
            listViewFunctionRoles.Items.Clear();
            listViewFunctionRoles.Items.AddRange(listViewItems);
        }

        public void FillRolesMainList(ListViewItem[] listViewItems)
        {
            listViewRolesMain.Items.Clear();
            listViewRolesMain.Items.AddRange(listViewItems);
        }

        public void FillDataPersonsList(ListViewItem[] listViewItems)
        {
            listViewDataPersons.Items.Clear();
            listViewDataPersons.Items.AddRange(listViewItems);
        }

        public void FillDataRolesList(ListViewItem[] listViewItems)
        {
            listViewDataRoles.Items.Clear();
            listViewDataRoles.Items.AddRange(listViewItems);
        }

        public void FillDataTree(TreeNodeAdv[] treeNodes, TreeNodeAdv[] dataTreeNodes, TreeNodeAdv[] allTreeNodes)
        {
            _allTreeNodeAdvs = allTreeNodes;
            treeViewData.Nodes.Clear();
            treeViewData.Nodes.AddRange(treeNodes);

            treeViewDataMain.Nodes.Clear();
            treeViewDataMain.Nodes.AddRange(dataTreeNodes);
        }

        public Guid SelectedPerson
        {
            get
            {
                var id = new Guid();
                if (listViewPersonsMain.SelectedItems.Count > 0)
                {
                    id = (Guid) listViewPersonsMain.SelectedItems[0].Tag;
                }
                return id;
            }
        }

        public ListView PersonRolesList
        {
            get { return  listViewPersonRoles; }
        }

        public ListView PersonFunctionsList
        {
            get { return listViewPersonFunctions; }
        }

        public ListView FunctionsMainList
        {
            get { return listViewFunctionsMain; }
        }

        public Guid SelectedFunction
        {
            get
            {
                var id = new Guid();
                if (listViewFunctionsMain.SelectedItems.Count > 0)
                {
                    id = (Guid)listViewFunctionsMain.SelectedItems[0].Tag;
                }
                return id;
            }
        }

        public ListView FunctionPersonsList
        {
            get { return listViewFunctionPersons; }
        }

        public ListView FunctionRolesList
        {
            get { return listViewFunctionRoles; }
        }

        private void listViewPersonsMainSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
        }


        private void listViewFunctionsMainSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
        }

        private void permissionViewerFormClosed(object sender, FormClosedEventArgs e)
        {
            _eventAggregator.GetEvent<PermissionsViewerUnloaded>().Publish("");
        }

        private void treeViewDataMainAfterSelect(object sender, EventArgs e)
        {
            var id = treeViewDataMain.SelectedNode.TagObject;
            if (id.GetType().Equals(typeof(Guid)))
            {
                _eventAggregator.GetEvent<DataPersonsAndRolesNeedLoad>().Publish("");
            }
            if (id.GetType().Equals(typeof(int)))
            {
                //_eventAggregator.GetEvent<DataRangePersonsAndRolesNeedLoad>().Publish("");
            }
            
        }
    }
}
