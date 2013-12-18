﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class StateGroupControl : BaseUserControl, ISettingPage
    {
        private IUnitOfWork _uow;
        private IList<IRtaStateGroup> _stateGroupCollection;
        // Helps keep track of the node that is being dragged (and which node has contextmenu visible).
        private TreeNodeAdv _currentSourceNode;
        private RtaStateGroupRepository _repository;

        public StateGroupControl()
        {
            InitializeComponent();

        	if (DesignMode) return;
        	buttonNew.Click += ButtonNewClick;
        	buttonDelete.Click += ButtonDeleteClick;
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonDelete, Resources.DeleteStateGroup);
            toolTip1.SetToolTip(buttonNew, Resources.NewStateGroup);
        }

        void ButtonDeleteClick(object sender, EventArgs e)
        {
			if (treeViewAdv1.SelectedNode == null)
				return;

			var stateGroupToDelete = treeViewAdv1.SelectedNode.TagObject as IRtaStateGroup;
			if(stateGroupToDelete != null)
				DeleteStateGroup(stateGroupToDelete);
			var stateToDelete = treeViewAdv1.SelectedNode.TagObject as IRtaState;
			if (stateToDelete != null)
				DeleteState(stateToDelete);
        }

        void ButtonNewClick(object sender, EventArgs e)
        {
            AddStateGroup();
        }

		private void DeleteState(IRtaState stateToDelete)
		{
			string text = string.Format(
				CurrentCulture,
				Resources.AreYouSureYouWantToDeleteItem,
				stateToDelete.Name);

			string caption = string.Format(CurrentCulture, Resources.ConfirmDelete);
			DialogResult response = ViewBase.ShowConfirmationMessage(text, caption);
			if (response != DialogResult.Yes) return;

			((IRtaStateGroup)stateToDelete.Parent).DeleteState(stateToDelete);
			treeViewAdv1.SelectedNode.Remove();
		}

		private void DeleteStateGroup(IRtaStateGroup stateGroupToDelete)
        {
            
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
        	}
        	IRtaStateGroup defaultStateGroup = GetDefaultStateGroup();
        	TreeNodeAdv nodeWithDefaultGroup = null;
			foreach (TreeNodeAdv node in treeViewAdv1.Nodes)
        	{
				var group = node.TagObject as IRtaStateGroup;
				if (group != null && group.DefaultStateGroup)
				{
					nodeWithDefaultGroup = node;
				}
        	}
        	foreach (var state in stateToMoveList)
        	{
        		stateGroupToDelete.MoveStateTo(defaultStateGroup, state);
				if(nodeWithDefaultGroup != null)
				{
					var nodeWithState = findNodeWithState(state, null);
					if(nodeWithState != null)
					{
						nodeWithState.Remove();
						nodeWithDefaultGroup.Nodes.Add(nodeWithState);
					}
				}
        	}
        	_repository.Remove(stateGroupToDelete);
        	_stateGroupCollection.Remove(stateGroupToDelete);
			treeViewAdv1.SelectedNode.Remove();
        	//UpdateTreeview(null, false);
        }

		private TreeNodeAdv findNodeWithState(IRtaState state, TreeNodeAdv node)
		{
			TreeNodeAdv retNode = null;
			if(node == null)
			{
				foreach (TreeNodeAdv treeNode in treeViewAdv1.Nodes)
				{
					retNode = findNodeWithState(state, treeNode);
					if (retNode != null) return retNode;
				}
			}
			if (node.TagObject.Equals(state))
				return node;
			foreach (TreeNodeAdv treeNodeAdv in node.Nodes)
			{
				retNode = findNodeWithState(state, treeNodeAdv);
				if (retNode != null) return retNode;
			}
			return retNode;
		}

        private void AddStateGroup()
        {
            IRtaStateGroup newStateGroup = new RtaStateGroup(Resources.NewStateGroup, false, false);
            _repository.Add(newStateGroup);
            _stateGroupCollection.Add(newStateGroup);
			var parentNode = (CreateNode(newStateGroup));
			treeViewAdv1.Nodes.Add(parentNode);
			treeViewAdv1.Root.Sort(TreeNodeAdvSortType.Text);
            //UpdateTreeview(newStateGroup, true);
        }

        private IRtaStateGroup GetDefaultStateGroup()
        {
            IRtaStateGroup returnValue = null;
            foreach (var stateGroup in _stateGroupCollection)
            {
                if (stateGroup.DefaultStateGroup)
                {
                    returnValue = stateGroup;
                }
            }

            return returnValue;
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            _uow = value;
        }

        public void Persist()
        {}

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

            tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            treeViewAdv1.BackColor = ColorHelper.GridControlGridInteriorColor();
        }

        public void LoadControl()
        {
            LoadStateGroups();
            InitializeTreeview();
            UpdateTreeview(null, false);
        }

        public void SaveChanges()
        {}
        
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

    	private void LoadStateGroups()
        {
            _repository = new RtaStateGroupRepository(_uow);
            _stateGroupCollection = _repository.LoadAllCompleteGraph();
        }

        private void InitializeTreeview()
        {
            treeViewAdv1.AllowDrop = true;
            treeViewAdv1.DragOnText = true;
            treeViewAdv1.LabelEdit = true;
            treeViewAdv1.AutoScrolling = ScrollBars.Vertical;
            treeViewAdv1.ContextMenu = new ContextMenu();
        }

        private void UpdateTreeview(IRtaStateGroup stateGroupToSelect, bool isNew)
        {
            treeViewAdv1.Nodes.Clear();
            TreeNodeAdv selectedNode = null;

            foreach (IRtaStateGroup stateGroup in _stateGroupCollection)
            {
                TreeNodeAdv parentNode = (CreateNode(stateGroup));

                foreach (IRtaState state in stateGroup.StateCollection)
                {
                    CreateChildNode(parentNode, state);
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

            if (isNew)
            {
                //treeViewAdv1.SelectedNode = newNode;
                treeViewAdv1.BeginEdit(selectedNode);
            }
            else
            {
                if (selectedNode != null)
                    treeViewAdv1.SelectedNode = selectedNode;
                else if (treeViewAdv1.Nodes.Count > 0)
                    treeViewAdv1.SelectedNode = treeViewAdv1.Nodes[0];
            }
        }

        private static TreeNodeAdv CreateNode(IRtaStateGroup stateGroup)
        {
            var node = new TreeNodeAdv(stateGroup.Name) {TagObject = stateGroup};
        	SetNodeToolTip(node);
            node.InteractiveCheckBox = true;

            node.ShowCheckBox = true;
            if (stateGroup.DefaultStateGroup)
            {
                node.Font = new Font(node.Font, FontStyle.Bold);
            }

            if (stateGroup.Available)
            {
                node.Checked = true;
                SetAvailableDescriptionToNode(node);
            }

            SetUseForLogOutDescriptionToNode(node);

            return node;
        }

        private static void CreateChildNode(TreeNodeAdv parentNode, IRtaState state)
        {
            var node = new TreeNodeAdv(state.Name) {TagObject = state};
        	parentNode.Nodes.Add(node);
        }

        private static void SetNodeToolTip(TreeNodeAdv node)
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

        private void TreeViewAdv1DragDrop(object sender, DragEventArgs e)
        {
            var treeView = sender as TreeViewAdv;

            if (treeView == null)
                return;

            // Get the destination and source node.
            var sourceNode = (TreeNodeAdv[])e.Data.GetData(typeof(TreeNodeAdv[]));

            Point pt = treeViewAdv1.PointToClient(new Point(e.X, e.Y));
            TreeNodeAdv destinationNode = treeViewAdv1.GetNodeAtPoint(pt);
            //sourceNode.Move(destinationNode, NodePositions.Next);
            MoveState(sourceNode, destinationNode);
			foreach (var node in sourceNode)
			{
				node.Move(destinationNode.Nodes, destinationNode.Nodes.Count);
			}
            
            _currentSourceNode = null;
			treeViewAdv1.Root.Sort(TreeNodeAdvSortType.Text);
            treeView.SelectedNode = sourceNode[0];

        }

        private void TreeViewAdv1DragOver(object sender, DragEventArgs e)
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
                var sourceNode = (TreeNodeAdv[])e.Data.GetData(typeof(TreeNodeAdv[]));

                _currentSourceNode = sourceNode[0];

                droppable = CanNodeBeDropped(sourceNode[0], destinationNode);

            }
            else
                droppable = false;

            e.Effect = droppable ? DragDropEffects.Move : DragDropEffects.None;

            var pt = treeViewAdv1.PointToClient(new Point(e.X, e.Y));
            treeViewAdv1.SelectedNode = treeViewAdv1.GetNodeAtPoint(pt);
        }

        private static bool CanNodeBeDropped(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
			if (destinationNode == null) return false;
            var sourceState = sourceNode.TagObject as IRtaState;
            var destinationStateGroup = destinationNode.TagObject as IRtaStateGroup;
            if (sourceState != null && destinationStateGroup != null && sourceState.StateGroup != destinationStateGroup)
                return true;

            return false;
        }

        private void treeViewAdv1_ItemDrag(object sender, ItemDragEventArgs e)
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
			if(node == null) return;
        	treeViewAdv.DoDragDrop(nodes, state != null ? DragDropEffects.Move : DragDropEffects.None);
        }

        private static void MoveState(TreeNodeAdv[] sourceNode, TreeNodeAdv destinationNode)
        {
        	foreach (var treeNodeAdv in sourceNode)
        	{
				var stateGroup = destinationNode.TagObject as IRtaStateGroup;
				var state = treeNodeAdv.TagObject as IRtaState;
				if (stateGroup != null && state != null)
				{
					treeNodeAdv.TagObject = state.StateGroup.MoveStateTo(stateGroup, state);
				}
        	}
        }

        private static void SetAvailableDescriptionToNode(TreeNodeAdv node)
        {
            var stateGroup = node.TagObject as IRtaStateGroup;
            if (stateGroup != null)
            {
                node.Text = node.Checked
                                ? string.Concat(stateGroup.Name, " - ", Resources.StateGroupAvailableDescription)
                                : stateGroup.Name;
            }
        }

        private static void SetUseForLogOutDescriptionToNode(TreeNodeAdv node)
        {
            var stateGroup = node.TagObject as IRtaStateGroup;
            if (stateGroup != null)
            {
                node.Text = stateGroup.IsLogOutState
                                ? string.Concat(node.Text, " - ", Resources.UseForLogOut)
                                : node.Text;
            }
        }

        private void treeViewAdv1_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Cancel dragging when Escape was pressed.
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
            }
        }

        private void treeViewAdv1_NodeEditorValidateString(object sender, TreeNodeAdvCancelableEditEventArgs e)
        {
            // Validating textchanges in edit textbox by user
            // Validate number of characters.
            if (e.Label.Length <= 50)
            {
                e.ContinueEditing = true;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void treeViewAdv1_NodeEditorValidating(object sender, TreeNodeAdvCancelableEditEventArgs e)
        {
            // Validating entered text by user
            // Check for empty name
            if (String.IsNullOrEmpty(e.Label) || e.Label.Trim().Length == 0)
            {
                // Invalid
                e.Cancel = true;
            }
            else
            {
                e.ContinueEditing = true;
            }
        }

        private void treeViewAdv1_NodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
        {
            // New text change is made - Save Name change in object
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
            if (stateGroup != null)
            {
                // State Group name changed
                stateGroup.Name = e.Node.Text;
                // If state group prop available is true then add description
                SetAvailableDescriptionToNode(e.Node);
                SetUseForLogOutDescriptionToNode(e.Node);
            }
            else
            {
                // State name changed
				var state = e.Node.TagObject as IRtaState;
                if (state != null) state.Name = e.Node.Text;
            }
        }

        private void treeViewAdv1_AfterCheck(object sender, TreeNodeAdvEventArgs e)
        {
            // The available checkbox is changed
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
        	if (stateGroup == null) return;
        	// State Group changed
        	stateGroup.Available = e.Node.Checked;
        	SetAvailableDescriptionToNode(e.Node);
        	SetUseForLogOutDescriptionToNode(e.Node);
        }

        private void TreeViewAdv1MouseDown(object sender, MouseEventArgs e)
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
        	BuildContextMenu();
        }

        private void BuildContextMenu()
        {
            //Create MenuItem for the SelectedNode
            var menuItemSetDefault = new MenuItem(Resources.SetAsDefaultStateGroup);
            var menuItemNewStateGroup = new MenuItem(Resources.NewStateGroup);
            var menuItemDeleteStateGroup = new MenuItem(Resources.DeleteStateGroup);
            var menuItemSetLogOut = new MenuItem(Resources.ToggleLogOutState);

            menuItemSetLogOut.Click += MenuItemSetLogOutClick;
            menuItemSetDefault.Click += MenuitemClick;
            menuItemNewStateGroup.Click += ButtonNewClick;
            menuItemDeleteStateGroup.Click += ButtonDeleteClick;

            treeViewAdv1.ContextMenu.MenuItems.Add(menuItemSetDefault);
            treeViewAdv1.ContextMenu.MenuItems.Add(menuItemSetLogOut);
            treeViewAdv1.ContextMenu.MenuItems.Add(menuItemNewStateGroup);
            treeViewAdv1.ContextMenu.MenuItems.Add(menuItemDeleteStateGroup);
        }

        private void MenuItemSetLogOutClick(object sender, EventArgs e)
        {
			var stateGroupToSet = _currentSourceNode.TagObject as IRtaStateGroup;
            if (stateGroupToSet == null) return;

            stateGroupToSet.IsLogOutState = !stateGroupToSet.IsLogOutState;
            UpdateTreeview(stateGroupToSet, false);
        }

        private void MenuitemClick(object sender, EventArgs e)
        {
            SetDefaultStateGroup();
        }

        private void SetDefaultStateGroup()
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
            UpdateTreeview(stateGroupToSet, false);
        }

        private void treeViewAdv1_BeforeEdit(object sender, TreeNodeAdvBeforeEditEventArgs e)
        {
			var stateGroup = e.Node.TagObject as IRtaStateGroup;
            if (stateGroup != null)
            {
                // be sure to remove ev. desc. in node text before editing state group name
                e.Node.Text = stateGroup.Name;
            }
        }

        private void treeViewAdv1_EditCancelled(object sender, TreeNodeAdvEditEventArgs e)
        {
            // If state group prop available is true then add description
            SetAvailableDescriptionToNode(e.Node);
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.StateGroup; }
        }	
    }
}
