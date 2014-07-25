using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class SettingsScreen : BaseRibbonFormWithUnitOfWork
	{
		private readonly OptionCore _core;
		private readonly Timer _timer = new Timer();

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
			//treeViewOptions.RightToLeft = RightToLeftLayout;
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
			treeViewOptions.AfterSelect += treeViewOptionsAfterSelect;
			Resize += settingsScreenResize;
			KeyPreview = true;
			KeyDown += formKeyDown;
			KeyPress += formKeyPress;
		}

		void treeViewOptionsAfterSelect(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				PanelContent.Controls.Clear();
				var item = (ISettingPage)treeViewOptions.SelectedNode.Tag;
				_core.MarkAsSelected(item, null);

				var userControl = item as Control;
				if (userControl != null)
				{
					userControl.Dock = DockStyle.Fill;
					PanelContent.Controls.Add(userControl);
					ActiveControl = userControl;
					setupHelpContext(userControl);
					item.OnShow();
				}
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
				formKillWithLatency();
			}

			Cursor.Current = Cursors.Default;
		}

		void formKeyDown(object sender, KeyEventArgs e)
		{
			if(ActiveControl != treeViewOptions) return;
			
			if (e.KeyValue.Equals(32))
			{
				e.Handled = true;
			}
		}

		void formKeyPress(object sender, KeyPressEventArgs e)
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
			loadTree(-1);
		}

		public SettingsScreen(OptionCore optionCore,SelectedEntity<IAggregateRoot> selectedEntity):this()
		{
			_core = optionCore;
			_core.SetUnitOfWork(UnitOfWork);
			TreeNodeAdv selectedTreeNode = loadTree(selectedEntity);

			Cursor.Current = Cursors.WaitCursor;
			PanelContent.Controls.Clear();

			var item = (ISettingPage)selectedTreeNode.Tag;
			_core.MarkAsSelected(item, selectedEntity);

			var userControl = item as Control;
			if (userControl != null)
			{
				userControl.Dock = DockStyle.Fill;
				PanelContent.Controls.Add(userControl);
				setupHelpContext(userControl);
				ActiveControl = userControl;
			}

			Cursor.Current = Cursors.Default;
		}

		protected void SetColors()
		{
			BackColor = ColorHelper.FormBackgroundColor();
		}

		void settingsScreenResize(object sender, EventArgs e)
		{
			// When using Syncfusions form a control with Anchor Top, Bottom, Left, Right
			// will not be resized correct

			if (PanelContent == null) return;
			PanelContent.Width = Width - 215;
			PanelContent.Height = Height - 100;
		}

		private void formKillWithLatency()
		{            
			_timer.Interval = 1000;
			_timer.Tick += timerTick;
			_timer.Start();
		}

		private void timerTick(object sender, EventArgs e)
		{
			FormKill();
		}

		private bool saveChanges()
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
				closeForm();
			}
			catch (ForeignKeyException)
			{
				//special case that comes as in bug 22347 -> FK-exception
				//the reason is most probably a deleted, but then used scorecard in SetScorecard

				ShowWarningMessage(UserTexts.Resources.DataHasBeenDeleted, UserTexts.Resources.OpenTeleoptiCCC);
				DialogResult = DialogResult.None;
				closeForm();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
				FormKill();
			}
			catch (Exception exception)
			{
				if (!(exception.InnerException is SqlException)) throw;
				if (exception.InnerException.Message.Contains("IX_KpiTarget"))
				{
					ShowWarningMessage(UserTexts.Resources.OptimisticLockText, UserTexts.Resources.OptimisticLockHeader);
					DialogResult = DialogResult.None;
					closeForm();
					return false;
				}
				throw;
			}
			return true;
		}

		private void closeForm()
		{
			_core.UnloadPages();
			KeyDown -= formKeyDown;
			KeyPress -= formKeyPress;
			Close();
			Dispose();
		}

		private void buttonApplyClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			saveChanges();
			
			DialogResult = DialogResult.None;
			Cursor.Current = Cursors.Default;
		}

		private void buttonOkClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (saveChanges())
			{
				closeForm();
				DialogResult = DialogResult.OK;
			}

			Cursor.Current = Cursors.Default;
		}

		private void buttonCancelClick(object sender, EventArgs e)
		{
			closeForm();
			DialogResult = DialogResult.Cancel;
		}

		private void loadTree(int nodeToSelect)
		{
			buildTree();

			if (nodeToSelect > 0)
				treeViewOptions.SelectedNode = treeViewOptions.Nodes[5];
			else
			{
				if (treeViewOptions.Nodes.Count >0)
					treeViewOptions.SelectedNode = treeViewOptions.Nodes[0];
			}
		}

		private TreeNodeAdv loadTree(SelectedEntity<IAggregateRoot> selectedEntity)
		{
			buildTree();

			TreeNodeAdv selected = getSelectedNode(selectedEntity);
			
			treeViewOptions.SelectedNode = selected ?? treeViewOptions.Nodes[0];

			return treeViewOptions.SelectedNode;
		}

		private void buildTree()
		{
			foreach (ISettingPage item in _core.AllSupportedPages)
			{
				TreeFamily treeFamily = item.TreeFamily();

				//Check for application permission
				if (!treeFamily.CheckPermission()) continue;

				var parent = getTreeNode(treeFamily.UserText);
				if (parent == null)
				{
					parent = new TreeNodeAdv(treeFamily.UserText) { Tag = item };
					treeViewOptions.Nodes.Add(parent);
				}
				var node = new TreeNodeAdv(item.TreeNode()) {Tag = item};
				parent.Nodes.Add(node);
				
			}
		}

		private TreeNodeAdv getSelectedNode(SelectedEntity<IAggregateRoot> entity)
		{
			foreach (TreeNodeAdv node in treeViewOptions.Nodes)
			{
				foreach (TreeNodeAdv childNodes in node.Nodes)
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

		private TreeNodeAdv getTreeNode(string nodeName)
		{
			return treeViewOptions.Nodes.Cast<TreeNodeAdv>().FirstOrDefault(item => item.Text == nodeName && item.Parent.Text == "root");
		}

		private void setupHelpContext(Control control)
		{
			RemoveControlHelpContext(control);
			AddControlHelpContext(control);
		}

		private void settingsScreenSizeChanged(object sender, EventArgs e)
		{
			// this is because syncfusion seems to fuck up the size when you set dock.fill so I handle it myself
			// otherwise the controls will come outside the form and you can't grab the border of the form and resize it
			 if(WindowState == FormWindowState.Minimized) return;
			var offset = 3;
			if (WindowState == FormWindowState.Maximized)
				offset = 6;
			tableLayoutPanel1.Height = Height - (ribbonControlAdv1.Height + offset +3);
			tableLayoutPanel1.Top = ribbonControlAdv1.Height +offset;
			tableLayoutPanel1.Left = 3;
			tableLayoutPanel1.Width = Width - 6;
		}
	}
}