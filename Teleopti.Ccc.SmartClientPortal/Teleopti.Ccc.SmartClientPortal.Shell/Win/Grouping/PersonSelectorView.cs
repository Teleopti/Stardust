using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.UserTexts;

using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
	public partial class PersonSelectorView : BaseUserControl, IPersonSelectorView
	{
		private  IEventAggregator _eventAggregator;
		private  IGroupPageHelper _groupPageHelper;
		private IComponentContext _container;
		private  IEventAggregator _globalEventAggregator;
		private DateOnlyPeriod? _selectedPeriod;
		private readonly Brush _intermediateBrush = Brushes.LightGray;

		public event EventHandler DoFilter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public PersonSelectorView(IEventAggregatorLocator eventAggregatorLocator, IGroupPageHelper groupPageHelper, IComponentContext container)
		{
			_eventAggregator = eventAggregatorLocator.LocalAggregator();
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();

			_groupPageHelper = groupPageHelper;
			_container = container;
			InitializeComponent();
			xdtpDate.Value = DateTime.Today;
			xdtpDate.SetCalendarMetroStyle();
			xdtpDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);			
			SetTexts();
			makeSureDropdownButtonWorksInRightToLeftCultures();
			PreselectedPersonIds = new HashSet<Guid>();
			treeViewAdvMainTabTree.SortWithChildNodes = true;
			treeViewAdvMainTabTree.Root.SortType = TreeNodeAdvSortType.Text;
			treeViewAdvMainTabTree.Root.SortOrder = SortOrder.Ascending;
	        treeViewAdvMainTabTree.AllowKeyboardSearch = true;
		}


		private void treeViewAdvMainTabTreeNodeBackgroundPaint(object sender, TreeNodeAdvPaintBackgroundEventArgs e)
		{
			if (e.Node.CheckState != CheckState.Indeterminate) return;
			e.Node.IntermediateCheckBoxBackGround = _intermediateBrush;
		}

		private void makeSureDropdownButtonWorksInRightToLeftCultures()
		{
			if (Thread.CurrentPrincipal is TeleoptiPrincipalForLegacy)
			{
				xdtpDate.RightToLeft = RightToLeft.No;
				xdtpDate.RightToLeft =
					(((ITeleoptiPrincipalForLegacy) TeleoptiPrincipal.CurrentPrincipal).UnsafePerson.PermissionInformation.RightToLeftDisplay)
						? RightToLeft.Yes
						: RightToLeft.No;
			}
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			if (tabControlAdv.TabPages != null)
				tabControlAdv.TabPages[0].Text = Resources.Main;
		}

		public void ResetTabs(TabPageAdv[] tabs, IExecutableCommand mainTabLoadCommand)
		{
			while (tabControlAdv.TabPages.Count > 1)
			{
				tabControlAdv.TabPages.RemoveAt(1);
			}
			SetCommonTexts();
			tabControlAdv.TabPages.AddRange(tabs);
			foreach (TabPageAdv tabPage in tabControlAdv.TabPages)
			{
				if (tabPage.Tag as ILoadUserDefinedTabsCommand != null)
					tabPage.Image = Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Add_16x16;
			}
			var tab = tabControlAdv.TabPages[0];
			tab.Tag = mainTabLoadCommand;
			tabControlAdv.SelectedIndex = 0;
			reLoadTree();
			tabControlAdv.Focus();
		}

		public DateOnly SelectedDate
		{
			get { return new DateOnly(xdtpDate.Value);}
		}

		public DateOnlyPeriod SelectedPeriod
		{
			get
			{
				return !_selectedPeriod.HasValue ? new DateOnlyPeriod(SelectedDate, SelectedDate) : _selectedPeriod.Value;
			}
			set { _selectedPeriod = value; }
		}
		
		public IList<TreeNodeAdv> SelectedNodes
		{
			get
			{
				return treeViewAdvMainTabTree.SelectedNodes.Cast<TreeNodeAdv>().ToList();
			}
		}

		public IList<TreeNodeAdv> AllNodes
		{
			get
			{
				return treeViewAdvMainTabTree.Nodes.Cast<TreeNodeAdv>().ToList();
			}
		}

		public TabPageAdv SelectedTab
		{
			get
			{
				return tabControlAdv?.SelectedTab;
			}
		}

		public TabControlAdv TabControl
		{
			get { return tabControlAdv; }
		}

		public bool KeepInteractiveOnDuringLoad { get; set; }

		public bool ExpandSelected { get; set; }

		public void ResetTreeView(TreeNodeAdv[] treeNodeAdv)
		{
			if(InvokeRequired)
			{
				BeginInvoke(new Action<TreeNodeAdv[]>(ResetTreeView), treeNodeAdv);
			}
			else
			{
				treeViewAdvMainTabTree.AfterCheck -= treeViewAdvMainTabTreeAfterCheck;
				treeViewAdvMainTabTree.BeginUpdate();
				if (!KeepInteractiveOnDuringLoad)
					treeViewAdvMainTabTree.InteractiveCheckBoxes = false;
				treeViewAdvMainTabTree.Nodes.Clear();
				treeViewAdvMainTabTree.Nodes.AddRange(treeNodeAdv);
				if (ShowCheckBoxes)
					treeViewAdvMainTabTree.InteractiveCheckBoxes = true;

				if(CheckParentNodesOnResetTree && treeViewAdvMainTabTree.Nodes.Count > 0)
					PersonSelectorPresenter.SetCheckStateForParentNodes(treeViewAdvMainTabTree.Nodes[0]);

				treeViewAdvMainTabTree.EndUpdate();
				treeViewAdvMainTabTree.AfterCheck += treeViewAdvMainTabTreeAfterCheck;
			}
		}

		public void ModifyGroupPage(Guid id)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				_groupPageHelper.SetSelectedPeriod(new DateOnlyPeriod(SelectedDate, SelectedDate));
				_groupPageHelper.LoadAll();
				_groupPageHelper.SetCurrentGroupPageById(id);

				var groupPagePreviewScreen = new GroupPagePreviewScreen(_groupPageHelper,_container);

				if (_groupPageHelper.CurrentGroupPage != null)
				{
					groupPagePreviewScreen.IsNewGroupPage = false;
					groupPagePreviewScreen.GroupPage = _groupPageHelper.CurrentGroupPage;

					groupPagePreviewScreen.ShowDialog(this);
					if (groupPagePreviewScreen.DialogResult == DialogResult.OK)
					{
						reLoadTree();
					}
				}
			}
			catch (DataSourceException exception)
			{
				ShowDataSourceException(exception, Resources.PersonAdmin);
			}
			Cursor = Cursors.Default;
	  }

		public void RenameGroupPage(Guid id, string oldName)
		{
			Cursor = Cursors.WaitCursor;
			var nameDialog = new NameDialog("xxRenameGroupPage", "xxEnterNewGroupName", oldName)
									{Text = Resources.RenameGroupPage};

			if (nameDialog.ShowDialog(this) == DialogResult.OK)
			{
				if (nameDialog.NameValue.Length > 0)
				{
					try
					{
						_groupPageHelper.RenameGroupPage(id, nameDialog.NameValue);
						_globalEventAggregator.GetEvent<GroupPageRenamed>().Publish(new EventGroupPage { Id = id, Name = nameDialog.NameValue });
					}
					catch (DataSourceException exception)
					{
						ShowDataSourceException(exception, Resources.PersonAdmin);
					}
				}
			}
			Cursor = Cursors.Default;
		}

		public void DeleteGroupPage(Guid id, string name )
		{
			DialogResult response = MessageDialogs.ShowQuestion(this,
																string.Format(CultureInfo.CurrentUICulture,
																				Resources.
																				GroupPageDeleteQuestion,
																				name),
																Resources.GroupPageDeleteQuestionCaption);

			if (response == DialogResult.Yes)
			{
				try
				{
					_groupPageHelper.RemoveGroupPageById(id);
					// send a message that a tab i removed so the others (used in people scheduler intraday?) can remove it too
					_globalEventAggregator.GetEvent<GroupPageDeleted>().Publish(new EventGroupPage { Id = id, Name = name });
				}
				catch (DataSourceException exception)
				{
					ShowDataSourceException(exception, Resources.PersonAdmin);
				}
			}
		}

		public void AddNewGroupPage()
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				using (var groupPageSettings = new GroupPageSettings(_groupPageHelper))
				{
					_groupPageHelper.SetSelectedPeriod(new DateOnlyPeriod(SelectedDate, SelectedDate));
					_groupPageHelper.LoadAll();
					if (groupPageSettings.ShowDialog(this) == DialogResult.OK)
					{
						if (groupPageSettings.NewGroupPage != null)
						{
							var newGroupPage = new EventGroupPage
							{
								Id = groupPageSettings.NewGroupPage.Id.Value,
								Name = groupPageSettings.NewGroupPage.Description.Name
							};
							_globalEventAggregator.GetEvent<GroupPageAdded>().Publish(newGroupPage);
						}
					}
				}
			}
			catch (DataSourceException exception)
			{
				ShowDataSourceException(exception, Resources.PersonAdmin);
			}
			Cursor = Cursors.Default;
		}

		public void AddNewPageTab(TabPageAdv tabPageAdv)
		{
			tabControlAdv.TabPages.Add(tabPageAdv);
			tabPageAdv.Image = Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Add_16x16;
			tabControlAdv.SelectedTab = tabPageAdv;
		}

		public void ShowDataSourceException(DataSourceException dataSourceException, string dialogTitle)
		{
			using (var view = new SimpleExceptionHandlerView(dataSourceException,
																	dialogTitle,
																	Resources.ServerUnavailable))
			{
				view.ShowDialog();
			}
		}

		public bool OpenEnabled
		{
			get { return OpenNewPPLAdmintoolStripMenuItem.Enabled; }
			set { OpenNewPPLAdmintoolStripMenuItem.Enabled = value; }
		}

		public bool FindVisible
		{
			get { return toolStripMenuItemSearch.Visible; }
			set { toolStripMenuItemSearch.Visible = value; }
		}

		public bool ShowCheckBoxes
		{
			get { return treeViewAdvMainTabTree.ShowCheckBoxes; }
			set
			{
				treeViewAdvMainTabTree.ShowCheckBoxes = value;
				CheckParentNodesOnResetTree = value;
			}
		}

		public bool CheckParentNodesOnResetTree { get; set; }

		public bool ShowDateSelection
		{
			get { return xdtpDate.Visible; }
			set { xdtpDate.Visible = value; }
		}

		public HashSet<Guid> PreselectedPersonIds { get; set;  }

		public IEnumerable<Guid> VisiblePersonIds { get; set; }

		private void tabControlAdvSelectedIndexChanged(object sender, EventArgs e)
		{
			var date = xdtpDate.Value;
			tabControlAdv.SelectedTab.Controls.Clear();
			treeViewAdvMainTabTree.RightToLeft = tabControlAdv.RightToLeft;
			tabControlAdv.SelectedTab.Controls.Add(treeViewAdvMainTabTree);
			tabControlAdv.SelectedTab.Controls.Add(xdtpDate);
			xdtpDate.Value = date;
			reLoadTree();
			treeViewAdvMainTabTree.HideSelection = false;
			treeViewAdvMainTabTree.SelectedNode = treeViewAdvMainTabTree.Nodes[0];
			_eventAggregator.GetEvent<SelectedNodesChanged>().Publish("");
		}

		private void reLoadTree()
		{
			var command = tabControlAdv.SelectedTab.Tag as IExecutableCommand;
			if (command == null) return;
			try
			{
				command.Execute();
			}
			catch (DataSourceException exception)
			{
				ShowDataSourceException(exception, Resources.PersonAdmin);
			}
			
		}

		private void xdtpDateValueChanged(object sender, EventArgs e)
		{
			if(xdtpDate.PopupWindow.IsShowing())
				return;
			reLoadTree();
		}

		private void xdtpDatePopupClosed(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		{
			reLoadTree();
		}

		private void openModule(object sender, EventArgs e)
		{
			_eventAggregator.GetEvent<OpenModuleClicked>().Publish("");
		}

		private void openModule(object sender, MouseEventArgs e)
		{
			var selectedNode = ((TreeViewAdv) sender).GetNodeAtPoint(e.X, e.Y);
			if (selectedNode != null && selectedNode.TextBounds.Contains(e.X, e.Y))
			_eventAggregator.GetEvent<OpenModuleClicked>().Publish("");
		}

		private void toolStripMenuItemSearchClick(object sender, EventArgs e)
		{
			_eventAggregator.GetEvent<FindPeopleClicked>().Publish("");
		}

		private void treeViewAdvMainTabTreeKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				OnDoFilter(new EventArgs());
				_eventAggregator.GetEvent<OpenModuleClicked>().Publish("");
				e.Handled = true;
			}
		}
	   
		private void treeViewAdvMainTabTreeAfterSelect(object sender, EventArgs e)
		{
			_eventAggregator.GetEvent<SelectedNodesChanged>().Publish("");
		}

		private void treeViewAdvMainTabTreeAfterCheck(object sender, TreeNodeAdvEventArgs e)
		{
			var agentIds = e.Node.Tag as IList<Guid>;
			if (agentIds != null && agentIds.Count == 1)
			{
				_eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Publish(new GroupPageNodeCheckData
				{
					AgentId = agentIds.First(),
					Node = e.Node
				});
			}
		}

		private void contextMenuStripOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (HideMenu)
				e.Cancel = true;
		}

		public  bool HideMenu { get; set; }


		private void refreshListToolStripMenuItemClick(object sender, EventArgs e)
		{
			_eventAggregator.GetEvent<RefreshGroupPageClicked>().Publish("");
		}

		protected virtual void OnDoFilter(EventArgs e)
		{
			EventHandler handler = DoFilter;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void tabControlAdvKeyPressed(object sender, KeyPressEventArgs e)
		{
			if (this.ActiveControl == treeViewAdvMainTabTree)
			{
				return;
			}

			int currentIndex = tabControlAdv.SelectedIndex;
			int maxIndex = tabControlAdv.TabCount-1;
			bool tabFound = false;
			string pressedKey = e.KeyChar.ToString(CultureInfo.InvariantCulture);
			if (currentIndex < maxIndex)
			{
				for (int i = currentIndex + 1; i <= maxIndex; i++)
				{
					tabFound = matchingTab(i, pressedKey);
					if (tabFound)
						break;
				}
			}
			if (currentIndex > 0 && !tabFound)
			{
				for (int i = 0; i < currentIndex; i++)
				{
					tabFound = matchingTab(i, pressedKey);
					if (tabFound)
						break;
				}
			}
		}

		private bool matchingTab(int i, string pressedKey)
		{
			bool tabFound = false;
			if (tabControlAdv.TabPages[i].Text.StartsWith(pressedKey, StringComparison.InvariantCultureIgnoreCase))
			{
				tabControlAdv.SelectedIndex = i;
				tabFound = true;
			}
			return tabFound;
		}

		private void tabControlAdvKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				treeViewAdvMainTabTreeKeyDown(this, e);
			}
		}

	}
}
