﻿using System;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class PermissionViewer : BaseDialogForm, IPermissionViewerRoles
    {
        private readonly IEventAggregator _eventAggregator;
        private TreeNodeAdv[] _allTreeNodeAdvs;
        private TreeNodeAdv[] _allFunctionTreeNodeAdvs;

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

        public TreeNodeAdv[] AllFunctionNodes
        {
            get { return _allFunctionTreeNodeAdvs; }
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

        public void FillFunctionTree(TreeNodeAdv[] personFunctionNodes, TreeNodeAdv[] mainFunctionNodes, TreeNodeAdv[] allTreeNodes)
        {
            _allFunctionTreeNodeAdvs = allTreeNodes;
            treeViewFunctionsMain.Nodes.Clear();
            treeViewFunctionsMain.Nodes.AddRange(mainFunctionNodes);

            treeViewPersonFunctions.Nodes.Clear();
            treeViewPersonFunctions.Nodes.AddRange(personFunctionNodes);
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

        public Guid SelectedFunction
        {
            get
            {
                var id = new Guid();
                if (treeViewFunctionsMain.SelectedNode != null)
                {
                    id = (Guid)treeViewFunctionsMain.SelectedNode.TagObject;
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

        public void ShowDataSourceException(Infrastructure.Foundation.DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.PermissionsViewer,
                                                                    Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        private void listViewPersonsMainSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<PersonRolesAndFunctionsNeedLoad>().Publish("");
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
               _eventAggregator.GetEvent<DataRangePersonsAndRolesNeedLoad>().Publish("");
            }
            
        }

        private void treeViewFunctionsMainAfterSelect(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
        }
    }
}
