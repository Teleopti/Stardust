using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class StateGroupControl : BaseUserControl, ISettingPage
	{
		private IList<IRtaStateGroup> _stateGroupCollection;
		private readonly IList<IRtaStateGroup> _removedGroups = new List<IRtaStateGroup>();
		private readonly IList<IRtaStateGroup> _groupsWithRemovedStates = new List<IRtaStateGroup>();

		private readonly StateGroupPersistHelper _persistHelper;

		private TreeNodeAdv _currentSourceNode;
		private bool _cancelValidate;

		private readonly IDisposable _rtaConfigurationIssuePolling;

		public StateGroupControl()
		{
			InitializeComponent();
			_persistHelper = new StateGroupPersistHelper(UnitOfWorkFactory.Current);

			if (DesignMode) return;
			buttonNew.Click += buttonNewClick;
			buttonDelete.Click += buttonDeleteClick;

			var displayHeight = tableLayoutPanelBody.RowStyles[1].Height;
			tableLayoutPanelBody.RowStyles[1].Height = 0;
			_rtaConfigurationIssuePolling = ContainerForLegacy.Container
				.Resolve<RtaConfigurationValidationPoller>()
				.Poll(messages =>
				{
					tableLayoutPanelBody.InvokeIfRequired(() =>
					{
						if (messages.Any())
						{
							tableLayoutPanelBody.RowStyles[1].Height = displayHeight;
							autoLabelRtaConfigurationValidation.Text = messages.First();
							toolTip1.SetToolTip(autoLabelRtaConfigurationValidation, string.Join("\n", messages));
						}
						else
						{
							tableLayoutPanelBody.RowStyles[1].Height = 0;
						}
					});
				});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing)
				_rtaConfigurationIssuePolling?.Dispose();
			base.Dispose(disposing);
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			treeViewAdv1.BackColor = ColorHelper.GridControlGridInteriorColor();
		}

		public void LoadControl()
		{
			_stateGroupCollection = _persistHelper.LoadStateGroupCollection();
			initializeTreeview();
			updateTreeview(null);
		}

		private void initializeTreeview()
		{
			treeViewAdv1.AllowDrop = true;
			treeViewAdv1.DragOnText = true;
			treeViewAdv1.LabelEdit = true;
			treeViewAdv1.AutoScrolling = ScrollBars.Vertical;
			treeViewAdv1.ContextMenu = new ContextMenu();
		}


		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDelete, Resources.DeleteStateGroup);
			toolTip1.SetToolTip(buttonNew, Resources.NewStateGroup);
		}


		private void buttonNewClick(object sender, EventArgs e)
		{
			addStateGroup();
		}

		private void addStateGroup()
		{
			IRtaStateGroup newStateGroup = new RtaStateGroup(Resources.NewStateGroup, false, false);
			_stateGroupCollection.Add(newStateGroup);
			var parentNode = (createNode(newStateGroup));
			treeViewAdv1.Nodes.Add(parentNode);
			treeViewAdv1.Root.Sort(TreeNodeAdvSortType.Text);
		}


		private void buttonDeleteClick(object sender, EventArgs e)
		{
			if (treeViewAdv1.SelectedNode == null)
				return;

			var stateGroupToDelete = treeViewAdv1.SelectedNode.TagObject as IRtaStateGroup;
			if (stateGroupToDelete != null)
				deleteStateGroup(stateGroupToDelete);
			var stateToDelete = treeViewAdv1.SelectedNode.TagObject as IRtaState;
			if (stateToDelete != null)
				deleteState(stateToDelete);
		}

		private void deleteState(IRtaState stateToDelete)
		{
			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteItem,
				stateToDelete.Name);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;

			((IRtaStateGroup) stateToDelete.Parent).DeleteState(stateToDelete);
			treeViewAdv1.SelectedNode.Remove();
		}

		private void deleteStateGroup(IRtaStateGroup stateGroupToDelete)
		{
			bool reloadTree = false;
			// A state group set as default can not be deleted
			if (stateGroupToDelete == null || stateGroupToDelete.DefaultStateGroup) return;

			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteItem,
				stateGroupToDelete.Name);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;

			//Before state group is deleted we must move its states to the default state group
			IList<IRtaState> stateToMoveList = new List<IRtaState>();
			foreach (var state in stateGroupToDelete.StateCollection)
			{
				stateToMoveList.Add(state);
				reloadTree = true;
			}

			var defaultStateGroup = _stateGroupCollection.LastOrDefault(x => x.DefaultStateGroup);
			var nodeWithDefaultGroup = (
					from TreeNodeAdv node in treeViewAdv1.Nodes
					let @group = node.TagObject as IRtaStateGroup
					where @group != null && @group.DefaultStateGroup
					select node)
				.SingleOrDefault();

			foreach (var state in stateToMoveList)
			{
				if (defaultStateGroup != null)
					defaultStateGroup.AddState(state.StateCode, state.Name);
				updateTreeViewAfterDelete(nodeWithDefaultGroup, state);
			}

			_removedGroups.Add(stateGroupToDelete);
			_stateGroupCollection.Remove(stateGroupToDelete);

			treeViewAdv1.SelectedNode.Remove();
			if (reloadTree)
				updateTreeview(null);
		}

		private void updateTreeViewAfterDelete(TreeNodeAdv nodeWithDefaultGroup, IRtaState state)
		{
			if (nodeWithDefaultGroup == null) return;
			var nodeWithState = findNodeWithState(state, null);
			if (nodeWithState == null) return;
			nodeWithState.Remove();
			nodeWithDefaultGroup.Nodes.Add(nodeWithState);
		}

		private TreeNodeAdv findNodeWithState(IRtaState state, TreeNodeAdv node)
		{
			TreeNodeAdv retNode;
			if (node == null)
			{
				foreach (TreeNodeAdv treeNode in treeViewAdv1.Nodes)
				{
					retNode = findNodeWithState(state, treeNode);
					if (retNode != null) return retNode;
				}
			}

			if (node != null)
			{
				if (node.TagObject.Equals(state) &&
					!((IRtaStateGroup) ((IRtaState) node.TagObject).Parent).DefaultStateGroup)
				{
					return node;
				}

				foreach (TreeNodeAdv treeNodeAdv in node.Nodes)
				{
					retNode = findNodeWithState(state, treeNodeAdv);
					if (retNode != null) return retNode;
				}
			}

			return null;
		}


		public void SetUnitOfWork(IUnitOfWork value)
		{
		}

		public void Persist()
		{
		}


		public void SaveChanges()
		{
			_persistHelper.Save(_stateGroupCollection, _removedGroups, _groupsWithRemovedStates);
			_removedGroups.Clear();
			_groupsWithRemovedStates.Clear();
		}

		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.RealTimeAdherence, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence);
		}

		public string TreeNode()
		{
			return Resources.StateGroupsAndStates;
		}

		public void OnShow()
		{
		}


		private void updateTreeview(IRtaStateGroup stateGroupToSelect)
		{
			treeViewAdv1.Nodes.Clear();
			TreeNodeAdv selectedNode = null;

			foreach (IRtaStateGroup stateGroup in _stateGroupCollection)
			{
				TreeNodeAdv parentNode = (createNode(stateGroup));

				foreach (IRtaState state in stateGroup.StateCollection)
				{
					createChildNode(parentNode, state);
				}

				treeViewAdv1.Nodes.Add(parentNode);

				if (stateGroupToSelect != null && stateGroupToSelect == parentNode.TagObject)
				{
					selectedNode = parentNode;
				}
			}

			treeViewAdv1.ExpandAll();

			//Sort tree
			treeViewAdv1.Root.SortOrder = SortOrder.Ascending;
			treeViewAdv1.SortWithChildNodes = true;
			treeViewAdv1.Root.Sort(TreeNodeAdvSortType.Text);

			if (selectedNode != null)
				treeViewAdv1.SelectedNode = selectedNode;
			else if (treeViewAdv1.Nodes.Count > 0)
				treeViewAdv1.SelectedNode = treeViewAdv1.Nodes[0];
		}


		private TreeNodeAdv createNode(IRtaStateGroup stateGroup)
		{
			var node = new TreeNodeAdv(stateGroup.Name) {TagObject = stateGroup};
			setNodeToolTip(node);
			node.InteractiveCheckBox = true;

			node.ShowCheckBox = true;
			if (stateGroup.DefaultStateGroup)
			{
				node.Font = new Font(node.Font, FontStyle.Bold);
			}

			if (stateGroup.Available)
			{
				node.Checked = true;
				setAvailableDescriptionToNode(node);
			}

			setUseForLogOutDescriptionToNode(node);

			return node;
		}

		private static void createChildNode(TreeNodeAdv parentNode, IRtaState state)
		{
			var node = new TreeNodeAdv(state.Name)
			{
				TagObject = state,
				HelpText = state.StateCode
			};
			parentNode.Nodes.Add(node);
		}

		private static void setNodeToolTip(TreeNodeAdv node)
		{
			var stateGroup = node.TagObject as IRtaStateGroup;
			if (stateGroup == null)
			{
				// If node is not a state group we don´t set tooltip
				return;
			}

			if (stateGroup.DefaultStateGroup)
			{
				node.HelpText = Resources.DefaultStateGroup;
			}
		}


		private void treeViewAdv1DragDrop(object sender, DragEventArgs e)
		{
			var treeView = sender as TreeViewAdv;

			if (treeView == null)
				return;

			// Get the destination and source node.
			var sourceNode = (TreeNodeAdv[]) e.Data.GetData(typeof(TreeNodeAdv[]));

			Point pt = treeViewAdv1.PointToClient(new Point(e.X, e.Y));
			TreeNodeAdv destinationNode = treeViewAdv1.GetNodeAtPoint(pt);
			//sourceNode.Move(destinationNode, NodePositions.Next);
			moveState(sourceNode, destinationNode);
			foreach (var node in sourceNode)
			{
				node.Move(destinationNode.Nodes);
			}

			_currentSourceNode = null;
			treeViewAdv1.Root.Sort(TreeNodeAdvSortType.Text);
			treeView.SelectedNode = sourceNode[0];
		}

		private void moveState(IEnumerable<TreeNodeAdv> sourceNode, TreeNodeAdv destinationNode)
		{
			foreach (var treeNodeAdv in sourceNode)
			{
				var stateGroup = destinationNode.TagObject as IRtaStateGroup;
				var state = treeNodeAdv.TagObject as IRtaState;
				if (stateGroup != null && state != null)
				{
					_groupsWithRemovedStates.Add(state.StateGroup);
					treeNodeAdv.TagObject = state.StateGroup.MoveStateTo(stateGroup, state);
				}
			}
		}

		private void treeViewAdv1DragOver(object sender, DragEventArgs e)
		{
			// Determine drag effects
			bool droppable;
			var treeView = sender as TreeViewAdv;
			if (treeView == null)
				return;

			var ptInTree = treeView.PointToClient(new Point(e.X, e.Y));
			_currentSourceNode = null;

			// Looking for a single tree node.
			if (e.Data.GetDataPresent(typeof(TreeNodeAdv[])))
			{
				// Get the destination and source node.
				var destinationNode = treeView.GetNodeAtPoint(ptInTree);
				var sourceNode = (TreeNodeAdv[]) e.Data.GetData(typeof(TreeNodeAdv[]));

				_currentSourceNode = sourceNode[0];

				droppable = canNodeBeDropped(sourceNode[0], destinationNode);
			}
			else
				droppable = false;

			e.Effect = droppable ? DragDropEffects.Move : DragDropEffects.None;

			var pt = treeViewAdv1.PointToClient(new Point(e.X, e.Y));
			treeViewAdv1.SelectedNode = treeViewAdv1.GetNodeAtPoint(pt);
		}

		private static bool canNodeBeDropped(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			if (destinationNode == null) return false;
			var sourceState = sourceNode.TagObject as IRtaState;
			var destinationStateGroup = destinationNode.TagObject as IRtaStateGroup;
			if (sourceState != null && destinationStateGroup != null && sourceState.StateGroup != destinationStateGroup)
				return true;

			return false;
		}

		private void treeViewAdv1ItemDrag(object sender, ItemDragEventArgs e)
		{
			var treeViewAdv = sender as TreeViewAdv;
			if (treeViewAdv == null)
				return;

			// The TreeViewAdv always provides an array of selected nodes.
			var nodes = e.Item as TreeNodeAdv[];
			if (nodes == null || nodes.Length == 0)
				return;

			// Let us get only the first selected node.
			var node = nodes[0];
			var state = node.TagObject as IRtaState;
			if (node == null) return;
			treeViewAdv.DoDragDrop(nodes, state != null ? DragDropEffects.Move : DragDropEffects.None);
		}


		private void setAvailableDescriptionToNode(TreeNodeAdv node)
		{
			_cancelValidate = true;
			var stateGroup = node.TagObject as IRtaStateGroup;
			if (stateGroup != null)
			{
				node.Text = node.Checked
					? string.Concat(stateGroup.Name, " - ", Resources.StateGroupAvailableDescription)
					: stateGroup.Name;
			}

			_cancelValidate = false;
		}

		private void setUseForLogOutDescriptionToNode(TreeNodeAdv node)
		{
			_cancelValidate = true;
			var stateGroup = node.TagObject as IRtaStateGroup;
			if (stateGroup != null)
			{
				node.Text = stateGroup.IsLogOutState
					? string.Concat(node.Text, " - ", Resources.UseForLogOut)
					: node.Text;
			}

			_cancelValidate = false;
		}

		private void treeViewAdv1QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			// Cancel dragging when Escape was pressed.
			if (e.EscapePressed)
			{
				e.Action = DragAction.Cancel;
			}
		}

		private void treeViewAdv1NodeEditorValidateString(object sender, TreeNodeAdvCancelableEditEventArgs e)
		{
			// Validating textchanges in edit textbox by user
			// Validate number of characters.
			if (e.Label.Length <= 50 && !_cancelValidate)
			{
				e.ContinueEditing = true;
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void treeViewAdv1NodeEditorValidating(object sender, TreeNodeAdvCancelableEditEventArgs e)
		{
			// Validating entered text by user
			// Check for empty name
			if (_cancelValidate || String.IsNullOrEmpty(e.Label) || e.Label.Trim().Length == 0)
			{
				// Invalid
				e.Cancel = true;
				e.ContinueEditing = false;
			}
			else
			{
				e.ContinueEditing = true;
			}
		}

		private void treeViewAdv1NodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
		{
			if (_cancelValidate) return;
			// New text change is made - Save Name change in object
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
			if (stateGroup != null)
			{
				// State Group name changed
				stateGroup.Name = e.Node.Text;
				// If state group prop available is true then add description
				setAvailableDescriptionToNode(e.Node);
				setUseForLogOutDescriptionToNode(e.Node);
			}
			else
			{
				// State name changed
				var state = e.Node.TagObject as IRtaState;
				if (state != null) state.Name = e.Node.Text;
			}
		}

		private void treeViewAdv1AfterCheck(object sender, TreeNodeAdvEventArgs e)
		{
			// The available checkbox is changed
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
			if (stateGroup == null) return;
			// State Group changed
			stateGroup.Available = e.Node.Checked;
			setAvailableDescriptionToNode(e.Node);
			setUseForLogOutDescriptionToNode(e.Node);
			_cancelValidate = false;
		}

		private void treeViewAdv1MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;
			// Clear "old" menuitems
			treeViewAdv1.ContextMenu.MenuItems.Clear();
			treeViewAdv1.SelectedNode = treeViewAdv1.GetNodeAtPoint(new Point(e.X, e.Y));
			//If the selected node is not equal to null, check for the condition
			if (treeViewAdv1.SelectedNode == null) return;
			// Only State group node should have contextmenu
			var stateGroup = treeViewAdv1.SelectedNode.TagObject as IRtaStateGroup;
			if (stateGroup == null) return;
			_currentSourceNode = treeViewAdv1.SelectedNode;
			buildContextMenu();
		}

		private void buildContextMenu()
		{
			//Create MenuItem for the SelectedNode
			var menuItemSetDefault = new MenuItem(Resources.SetAsDefaultStateGroup);
			var menuItemNewStateGroup = new MenuItem(Resources.NewStateGroup);
			var menuItemDeleteStateGroup = new MenuItem(Resources.DeleteStateGroup);
			var menuItemSetLogOut = new MenuItem(Resources.ToggleLogOutState);

			menuItemSetLogOut.Click += menuItemSetLogOutClick;
			menuItemSetDefault.Click += menuitemClick;
			menuItemNewStateGroup.Click += buttonNewClick;
			menuItemDeleteStateGroup.Click += buttonDeleteClick;

			treeViewAdv1.ContextMenu.MenuItems.Add(menuItemSetDefault);
			treeViewAdv1.ContextMenu.MenuItems.Add(menuItemSetLogOut);
			treeViewAdv1.ContextMenu.MenuItems.Add(menuItemNewStateGroup);
			treeViewAdv1.ContextMenu.MenuItems.Add(menuItemDeleteStateGroup);
		}

		private void menuItemSetLogOutClick(object sender, EventArgs e)
		{
			var stateGroupToSet = _currentSourceNode.TagObject as IRtaStateGroup;
			if (stateGroupToSet == null) return;

			stateGroupToSet.IsLogOutState = !stateGroupToSet.IsLogOutState;
			updateTreeview(stateGroupToSet);
		}

		private void menuitemClick(object sender, EventArgs e)
		{
			setDefaultStateGroup();
		}

		private void setDefaultStateGroup()
		{
			var stateGroupToSet = _currentSourceNode.TagObject as IRtaStateGroup;

			if (stateGroupToSet == null)
				return;

			// Set DefaultStateGroup property for all state groups to false, except for the given state group.
			foreach (IRtaStateGroup stateGroup in _stateGroupCollection)
			{
				if (stateGroup != stateGroupToSet)
				{
					stateGroup.DefaultStateGroup = false;
				}
			}

			stateGroupToSet.DefaultStateGroup = true;
			updateTreeview(stateGroupToSet);
		}

		private void treeViewAdv1BeforeEdit(object sender, TreeNodeAdvBeforeEditEventArgs e)
		{
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
			if (stateGroup != null)
			{
				// be sure to remove ev. desc. in node text before editing state group name
				e.Node.Text = stateGroup.Name;
			}
		}

		private void treeViewAdv1EditCancelled(object sender, TreeNodeAdvEditEventArgs e)
		{
			// If state group prop available is true then add description
			setAvailableDescriptionToNode(e.Node);
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.StateGroup; }
		}

		private void treeViewAdv1BeforeCheck(object sender, TreeNodeAdvBeforeCheckEventArgs e)
		{
			treeViewAdv1.SelectedNode = e.Node;
			_cancelValidate = true;
		}
	}
}