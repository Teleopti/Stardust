using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Permissions;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Settings
{
    public partial class AdminNavigationPanel : BaseUserControlWithUnitOfWork
    {
        
        public AdminNavigationPanel()
        {
            InitializeComponent();
            linkLabelPermissions.LinkClicked +=new LinkLabelLinkClickedEventHandler(linkLabelPermissions_LinkClicked);
            linkLabelShifts.LinkClicked +=new LinkLabelLinkClickedEventHandler(linkLabelShifts_LinkClicked);
            SetTexts();
           
        }

        private void linkLabelPermissions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // TODO: delete commented part later
            //Form frm = new Form();
            //frm.Text = "Permissions Dummy Form";
            //frm.Size = new Size(500, 1000);
            //PermissionNavigationPanel _permissionNavigationPanel = new PermissionNavigationPanel();
            //frm.Controls.Add(_permissionNavigationPanel);
            //_permissionNavigationPanel.ExchangeData(ExchangeDataOption.ServerToClient);
            //_permissionNavigationPanel.Dock = DockStyle.Fill;
            //frm.ShowDialog();

            // TODO: This is the correct form for Permission.
            //Permissions.PermissionsStateHolder stateHolder = new PermissionsStateHolder();
            Permissions.PermissionsExplorer permissionForm = new PermissionsExplorer();
            permissionForm.Show();
        }

        private void linkLabelShifts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Form frm = new Form();
            //frm.Text = "Shifts Dummy Form";
            //frm.ShowDialog();

            Cursor.Current = Cursors.WaitCursor;
            IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();

            ShiftCreator.ExplorerStateHolder state = new Teleopti.Ccc.Win.ShiftCreator.ExplorerStateHolder(unitOfWork);
            state.LoadWorkShiftRuleSets();
            state.LoadActivityCollection();
            state.LoadCategoryCollection();
            state.LoadClassTypeCollection();

            // HACK: Assuming this is the correct link for shift creator.
            //ShiftCreator.WorkShiftsExplorer workShiftForm = new ShiftCreator.WorkShiftsExplorer(state);
            //workShiftForm.Show();

            Shifts.WorkShiftsExplorer sc = new Shifts.WorkShiftsExplorer(unitOfWork);
            sc.Show();

            Cursor.Current = Cursors.Default;
        }

        private void linkLabelSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form frm = new SettingsScreen(-1);
            frm.Show();
        }
    }
}
