using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class SettingsScreen : BaseRibbonFormWithUnitOfWork
    {
        private readonly OptionCore _core;
        private Timer _timer = new Timer();

        protected SettingsScreen()
        {
            // Initializes the settings screen components
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
            }
            if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
                Icon = Properties.Resources.options;
            // Sets colors for form & controls.
            SetColors();

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            treeViewOptions.RightToLeftLayout = RightToLeftLayout;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            treeViewOptions.AfterSelect += TreeViewOptionsAfterSelect;
            Resize += SettingsScreenResize;
        	KeyPreview = true;
			KeyDown += Form_KeyDown;
			KeyPress += Form_KeyPress;
		}

		void Form_KeyDown(object sender, KeyEventArgs e)
		{
			if(ActiveControl != treeViewOptions) return;
			
			if (e.KeyValue.Equals(32))
			{
				e.Handled = true;
			}
		}

		void Form_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (ActiveControl != treeViewOptions) return;

			if (e.KeyChar.Equals((Char)Keys.Space))
			{
				e.Handled = true;
			}
		}

        public SettingsScreen(OptionCore optionCore)
            : this()
        {
            // Loads tree values.
            _core = optionCore;
            _core.SetUnitOfWork(UnitOfWork);
            LoadTree(-1);
        }

        public SettingsScreen(OptionCore optionCore,SelectedEntity<IAggregateRoot> selectedEntity):this()
        {
            _core = optionCore;
            _core.SetUnitOfWork(UnitOfWork);
            TreeNode selectedTreeNode = LoadTree(selectedEntity);

            Cursor.Current = Cursors.WaitCursor;
            PanelContent.Controls.Clear();

            var item = (ISettingPage)selectedTreeNode.Tag;
            _core.MarkAsSelected(item, selectedEntity);

            var userControl = item as Control;
            if (userControl != null)
            {
                userControl.Dock = DockStyle.Fill;
                PanelContent.Controls.Add(userControl);
                SetupHelpContext(userControl);
                ActiveControl = userControl;
            }

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Sets the colors.
        /// </summary>
        protected void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
        }

        void SettingsScreenResize(object sender, EventArgs e)
        {
            // When using Syncfusions form a control with Anchor Top, Bottom, Left, Right
            // will not be resized correct

        	if (PanelContent == null) return;
        	PanelContent.Width = Width - 215;
        	PanelContent.Height = Height - 100;
        }

        void TreeViewOptionsAfterSelect(object sender, TreeViewEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                PanelContent.Controls.Clear();
                var item = (ISettingPage)e.Node.Tag;
                _core.MarkAsSelected(item, null);

                var userControl = item as Control;
                if (userControl != null)
                {
                    userControl.Dock = DockStyle.Fill;
                    PanelContent.Controls.Add(userControl);
                    ActiveControl = userControl;
                    SetupHelpContext(userControl);
                    item.OnShow();
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKillWithLatency();
            }

            Cursor.Current = Cursors.Default;
        }

        private void FormKillWithLatency()
        {            
            _timer.Interval = 1000;
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            FormKill();
        }

        private bool SaveChanges()
        {
			try
			{
				_core.SaveChanges();
			}
			catch (ValidationException ex)
			{
				ShowWarningMessage(UserTexts.Resources.InvalidDataOnFollowingPage + ex.Message, UserTexts.Resources.ValidationError);
				DialogResult = DialogResult.None;
				return false;
			}
			catch (OptimisticLockException)
			{
				ShowWarningMessage(UserTexts.Resources.OptimisticLockText, UserTexts.Resources.OptimisticLockHeader);
				DialogResult = DialogResult.None;
				CloseForm();
			}
			catch (ForeignKeyException)
			{
				//special case that comes as in bug 22347 -> FK-exception
				//the reason is most probably a deleted, but then used scorecard in SetScorecard

				ShowWarningMessage(UserTexts.Resources.DataHasBeenDeleted, UserTexts.Resources.OpenTeleoptiCCC);
				DialogResult = DialogResult.None;
				CloseForm();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
				FormKill();
			}
			catch (Exception exception)
			{
				if (exception.InnerException != null && exception.InnerException is SqlException)
					if (exception.InnerException.Message.Contains("IX_KpiTarget"))
					{
						ShowWarningMessage(UserTexts.Resources.OptimisticLockText, UserTexts.Resources.OptimisticLockHeader);
						DialogResult = DialogResult.None;
						CloseForm();
						return false;
					}
				throw;
			}
            return true;
        }

        private void CloseForm()
        {
            _core.UnloadPages();
			KeyDown -= Form_KeyDown;
			KeyPress -= Form_KeyPress;
            Close();
            Dispose();
        }

        private void ButtonApplyClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            SaveChanges();
            
            DialogResult = DialogResult.None;
            Cursor.Current = Cursors.Default;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (SaveChanges())
            {
                CloseForm();
                DialogResult = DialogResult.OK;
            }

            Cursor.Current = Cursors.Default;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            CloseForm();
            DialogResult = DialogResult.Cancel;
        }

        private void LoadTree(int nodeToSelect)
        {
            BuildTree();

            if (nodeToSelect > 0)
                treeViewOptions.SelectedNode = treeViewOptions.Nodes[5];
            else
            {
                if (treeViewOptions.Nodes.Count >0)
                    treeViewOptions.SelectedNode = treeViewOptions.Nodes[0];
            }
        }

        private TreeNode LoadTree(SelectedEntity<IAggregateRoot> selectedEntity)
        {
            BuildTree();

            TreeNode selected = GetSelectedNode(selectedEntity);
            
            treeViewOptions.SelectedNode = selected ?? treeViewOptions.Nodes[0];

            return treeViewOptions.SelectedNode;
        }

        private void BuildTree()
        {
            foreach (ISettingPage item in _core.AllSupportedPages)
            {
                TreeFamily treeFamily = item.TreeFamily();

                //Check for application permission
                if (!treeFamily.CheckPermission()) continue;

                TreeNode parent = GetTreeNode(treeFamily.UserText);
                if (parent == null)
                {
                    parent = treeViewOptions.Nodes.Add(treeFamily.UserText);
                    parent.Tag = item;
                }
                TreeNode node = parent.Nodes.Add(item.TreeNode());
                node.Tag = item;
            }
        }

        private TreeNode GetSelectedNode(SelectedEntity<IAggregateRoot> entity)
        {
            foreach (TreeNode node in treeViewOptions.Nodes)
            {
                foreach (TreeNode childNodes in node.Nodes)
                {
                    var control = childNodes.Tag as ISettingPage;

                    if (control != null && control.ViewType == entity.ViewType)
                    {
                        return childNodes;
                    }
                }
            }

            return null;
        }

        private TreeNode GetTreeNode(string nodeName)
        {
            foreach (TreeNode item in treeViewOptions.Nodes)
            {
                if (item.Text == nodeName && item.Parent == null)
                    return item;
            }
            return null;
        }

        private void SetupHelpContext(Control control)
        {
            RemoveControlHelpContext(control);
            AddControlHelpContext(control);
        }

        private void treeViewOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

    }
}