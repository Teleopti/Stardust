using System;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Ccc.WinCode.Permissions.Events;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class PermissionViewerRoles : Form, IPermissionViewerRoles
    {
        private readonly IEventAggregator _eventAggregator;

        public PermissionViewerRoles(IEventAggregator eventAggregator):this()
        {
            _eventAggregator = eventAggregator;
        }

        public PermissionViewerRoles()
        {
            InitializeComponent();
        }

        public ListView RolesMainList
        {
            get { return listViewRolesMain; }
        }

        public Guid SelectedRole
        {
            get { throw new NotImplementedException(); }
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

        private void permissionViewerRolesFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void listViewFunctionsMainSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<FunctionPersonsAndRolesNeedLoad>().Publish("");
        }
    }
}
