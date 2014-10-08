using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Grouping
{
    /// <summary>
    /// Represent the GroupPagePreviewScreen 
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-06-26
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class GroupPagePreviewScreen : BaseDialogForm, ISelfDataHandling
    {
        /// <summary>
        /// Values of the different Levels of the tree nodes
        /// </summary>
        private enum TreeElementLevel
        {
            /// <summary>
            /// Virtual Root in the tree
            /// </summary>
            VirtualRoot,
            /// <summary>
            /// Root in the tree
            /// </summary>
            Root,
            /// <summary>
            /// Children in the tree
            /// </summary>
            Children
        }

        private TreeNodeAdv _currentSourceNode; // Helps keep track of the node that is being dragged.
        private TreeViewAdvDragHighlightTracker _treeViewDragHighlightTracker;
        private readonly Color _highlighter = Color.Red;
        private SelectedNodesCollection _rightMouseDownNodeCached;
        private readonly TreeViewCreator _treeViewCreator;
        private SelectedNodesCollection _cutSourceNode;

        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                   DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

        private readonly IGroupPageHelper _groupPageHelper;

        public string GroupPageName { get; set; }

        public DynamicOptionType GroupPageType { get; set; }

        public IGroupPage GroupPage { get; set; }

        public bool IsNewGroupPage { get; set; }

        public Guid? OptionalColumnId { get; set; }

        public void Persist()
        {
        }

        public GroupPagePreviewScreen()
        {
            GroupPageName = string.Empty;
            InitializeComponent();

            if (!DesignMode)
            {
                SetTexts();
            }
        }

        public GroupPagePreviewScreen(IGroupPageHelper groupPageHelper)
            : this()
        {
            _groupPageHelper = groupPageHelper;
            _treeViewCreator = new TreeViewCreator(_groupPageHelper);
            treeViewAdvPreviewTree.AutoScrolling = ScrollBars.Vertical;
        }

        void treeViewAdvPreviewTree_NodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
        {
            var group = (PersonGroupBase)e.Node.TagObject;
			var len = e.Node.Text.Length;
			if (len > 50) len = 50;
        	e.Node.Text = e.Node.Text.Substring(0, len);
            group.Description = new Description(e.Node.Text);
        }

        /// <summary>
        /// Handles the Load event of the GroupPagePreviewScreen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-06-26
        /// </remarks>
        private void GroupPagePreviewScreen_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            LoadGroupPage();

            _treeViewDragHighlightTracker = new TreeViewAdvDragHighlightTracker(treeViewAdvPreviewTree);

            _treeViewDragHighlightTracker.QueryAllowedPositionsForNode += TreeDragDropQueryAllowedPositionsForNode;
            _treeViewDragHighlightTracker.QueryDragInsertInfo += TreeViewDragHighlightTrackerQueryDragInsertInfo;


            findPersonsView1.Initialize(new FindPersonsModel(_groupPageHelper.PersonCollection), _myApplicationFunction);
            SetTexts();

        }

        /// <summary>
        /// Handles the DragOver event of the treeViewAdvPreviewTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-06-26
        /// </remarks>
        private void treeViewAdvPreviewTree_DragOver(object sender, DragEventArgs e)
        {
            // Determine drag effects
            TreeNodeAdv destinationNode = null;
            TreeViewAdv treeView = sender as TreeViewAdv;

            if (treeView != null)
            {
                Point pointInTree = treeView.PointToClient(new Point(e.X, e.Y));
                _currentSourceNode = null;

                // Looking for a single tree node.
                bool droppable;
                if (e.Data.GetDataPresent(typeof(TreeNodeAdv)))
                {
                    // Get the destination and source node.
                    destinationNode = treeView.GetNodeAtPoint(pointInTree);
                    TreeNodeAdv sourceNode = (TreeNodeAdv)e.Data.GetData(typeof(TreeNodeAdv));
                    // Cache this for use later in the treeViewAdvPreviewTree_QueryAllowedPositionsForNode event handler.
                    _currentSourceNode = sourceNode;
                    droppable = true;// CanDrop(sourceNode, destinationNode);
                }
                else
                    droppable = false;

                if (droppable)
                {
                    // If Moving is allowed:
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }

                // Let the highlight tracker keep track of the current highlight node.
                _treeViewDragHighlightTracker.SetHighlightNode(destinationNode, pointInTree);
            }
        }

        /// <summary>
        /// Handles the DragLeave event of the treeViewAdvPreviewTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void treeViewAdvPreviewTree_DragLeave(object sender, EventArgs e)
        {
            // Let the highlight tracker keep track of the current highlight node.
            _treeViewDragHighlightTracker.ClearHighlightNode();
        }

        /// <summary>
        /// Handles the DragDrop event of the treeViewAdvPreviewTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void treeViewAdvPreviewTree_DragDrop(object sender, DragEventArgs e)
        {
            TreeViewAdv treeView = sender as TreeViewAdv;

            if (treeView != null)
            {
                // Get the source node.
                TreeNodeAdv destinationNode = _treeViewDragHighlightTracker.HighlightNode;
                TreeViewDropPositions dropPosition = _treeViewDragHighlightTracker.DropPosition;
                // Clear the highlight info in the tracker.
                _treeViewDragHighlightTracker.ClearHighlightNode();
                _currentSourceNode = null;

                // Move the source node based on the tracked info.
                if (destinationNode != null)
                {
                    // Get the destination node 
                    TreeNodeAdv sourceNode = (TreeNodeAdv)e.Data.GetData(typeof(TreeNodeAdv));

	                if (!IsPersonNode(sourceNode))
	                {
						if(sourceNode.HasNode(destinationNode) || sourceNode == destinationNode) return;   
	                }

                    //Clone the nodes to avoid the override issue
                    var selectedNodesCollection = (SelectedNodesCollection)sourceNode.TreeView.SelectedNodes.Clone();

                    foreach (TreeNodeAdv node in selectedNodesCollection)
                    {
                        var nodeInDestinationTree = treeViewAdvPreviewTree.FindNodesWithTagObject(node.TagObject).First();
                      
                        switch (dropPosition)
                        {
                            case TreeViewDropPositions.AboveNode:
                                {
                                    PerformNodeMoveInGroupPage(nodeInDestinationTree, destinationNode.Parent);
                                    nodeInDestinationTree.Move(destinationNode, NodePositions.Previous);
                                    break;
                                }
                            case TreeViewDropPositions.BelowNode:
                                {
                                    PerformNodeMoveInGroupPage(nodeInDestinationTree, destinationNode.Parent);
                                    nodeInDestinationTree.Move(destinationNode, NodePositions.Next);
                                    break;
                                }
                            case TreeViewDropPositions.OnNode:
                                {
                                    PerformNodeMoveInGroupPage(nodeInDestinationTree, destinationNode);
                                    nodeInDestinationTree.Move(destinationNode.Nodes);
                                    destinationNode.Expand();
                                    break;
                                }
                        }
                    }

                    //treeView.SelectedNode = sourceNode;
                }
            }
        }

        /// <summary>
        /// Handles the ItemDrag event of the treeViewAdvPreviewTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ItemDragEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void treeViewAdvPreviewTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeViewAdv treeView = sender as TreeViewAdv;

            if (treeView != null)
            {
                // The TreeViewAdv always provides an array of selected nodes.
                TreeNodeAdv[] nodes = e.Item as TreeNodeAdv[];

                if (nodes != null && nodes.Length >= 1)
                {
                    //check whether the root node also selected
                    if (IsRootSelected(nodes))
                    {
                        QueryContinueDragEventArgs ea = new QueryContinueDragEventArgs(1, true, DragAction.Continue);
                        treeViewAdvPreviewTree_QueryContinueDrag(treeViewAdvPreviewTree, ea);
                        return;
                    }

                    // Let us get only the first selected node.
                    TreeNodeAdv node = nodes[0];
                    // Only allow move
                    treeView.DoDragDrop(node, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Handles the QueryContinueDrag event of the treeViewAdvPreviewTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.QueryContinueDragEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void treeViewAdvPreviewTree_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Cancel dragging when Escape was pressed.
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
            }
        }

        /// <summary>
        /// Handles the QueryAllowedPositionsForNode event of the TreeDragDrop control.
        /// Specifiy the allowed drop positions for the specified highlight node.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Tools.QueryAllowedPositionsEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void TreeDragDropQueryAllowedPositionsForNode(object sender, QueryAllowedPositionsEventArgs e)
        {
            if (IsPersonNode(e.HighlightNode))
            {
                // If this a person node, only allow drop above or below it.
                if (e.HighlightNode != _currentSourceNode)
                    e.AllowedPositions = TreeViewDropPositions.AboveNode | TreeViewDropPositions.BelowNode;
                else
                    // Cannot drop beside itself
                    e.AllowedPositions = TreeViewDropPositions.None;

                _treeViewDragHighlightTracker.EdgeSensitivityOnTop =
                    _treeViewDragHighlightTracker.EdgeSensitivityAtBottom = e.HighlightNode.Bounds.Height / 2;
                e.ShowSelectionHighlight = false;
            }
            else
            {
                // If this is a group node allow all drop positions (default behavior).
                _treeViewDragHighlightTracker.EdgeSensitivityOnTop =
                    _treeViewDragHighlightTracker.EdgeSensitivityAtBottom = e.HighlightNode.Bounds.Height / 4;
                e.ShowSelectionHighlight =
                    // Only if the source node is droppable
                    CanDrop(_currentSourceNode, e.HighlightNode)
                    // and droppable ON the node (not beside it)
                    && e.NewDropPosition == TreeViewDropPositions.OnNode;
            }
        }

        /// <summary>
        /// Handles the QueryDragInsertInfo event of the _treeViewDragHighlightTracker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="Syncfusion.Windows.Forms.Tools.QueryDragInsertInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void TreeViewDragHighlightTrackerQueryDragInsertInfo(object sender, QueryDragInsertInfoEventArgs args)
        {
            args.DragInsertColor = _highlighter;
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void buttonAdvOk_Click(object sender, EventArgs e)
        {

            _groupPageHelper.UpdateCurrent();

            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemNewGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void toolStripMenuItemNewGroup_Click(object sender, EventArgs e)
        {
            HandleCreateNewGroup();
        }

        /// <summary>
        /// Handles the Opening event of the contextMenuStripTreeActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void contextMenuStripTreeActions_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
            toolStripMenuItemCut.Enabled = true;
            toolStripMenuItemPaste.Enabled = true;
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemChangeNameOnGroup.Enabled = false;
            if (_rightMouseDownNodeCached == null) return;
            if (_cutSourceNode == null && findPersonsView1.CutNodes == null)
                toolStripMenuItemPaste.Enabled = false;
            if (string.Compare(GroupingConstants.NodeTypeRoot, _rightMouseDownNodeCached[0].Tag.ToString(),
                               StringComparison.OrdinalIgnoreCase) == 0)
            {
                toolStripMenuItemCut.Enabled = false;
            }
            else if (string.Compare(GroupingConstants.NodeTypePerson, _rightMouseDownNodeCached[0].Tag.ToString(),
                               StringComparison.OrdinalIgnoreCase) == 0)
            {
                toolStripMenuItemDelete.Enabled = CanPerformPersonDeletion(_rightMouseDownNodeCached[0]); 
            }
            else if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
                                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                toolStripMenuItemDelete.Enabled = CanPerformGroupDeletion(_rightMouseDownNodeCached[0]); 
            }
            if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
                               StringComparison.OrdinalIgnoreCase) == 0)
            {
                toolStripMenuItemChangeNameOnGroup.Enabled = true;
            }
        }


        /// <summary>
        /// Handles the Click event of the toolStripMenuItemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-09
        /// </remarks>
        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            handleDeletion();
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
        //    {
        //        _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNode;
        //        HandleCut();
        //    }

        //    if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
        //    {
        //        _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNode;
        //        HandlePaste();
        //    }

        //    base.OnKeyDown(e);
        //}

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemCut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemCut_Click(object sender, EventArgs e)
        {
            handleCut();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemPaste control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemPaste_Click(object sender, EventArgs e)
        {
            handlePaste();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemChangeNameOnGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemChangeNameOnGroup_Click(object sender, EventArgs e)
        {
            HandleChangeNameOnGroup();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemCollapseAllNodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemCollapseAllNodes_Click(object sender, EventArgs e)
        {
            handleCollapseAllNodes();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemExpandOnlyRootNodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemExpandOnlyRootNodes_Click(object sender, EventArgs e)
        {
            handleExpandOnlyRootNodes();

        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemExpandAllNodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        private void toolStripMenuItemExpandAllNodes_Click(object sender, EventArgs e)
        {
            handleExpandAllNodes();

        }

        /// <summary>
        /// Loads the group page.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void LoadGroupPage()
        {
            if (GroupPage != null)
            {
                //tabControlAdvPreviewTab.TabPages[0].Tag = GroupPage;
                //tabControlAdvPreviewTab.TabPages[0].Text = "";

                //tabControlAdvPreviewTab.TabPages[0].TabVisible = false;
                //retrieve tree nodes.
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					uow.Reassociate(GroupPage);
					IList<TreeNodeAdv> treeNodes = _treeViewCreator.CreateView(GroupPage, _myApplicationFunction);
				    GroupPageName = GroupPage.Description.Name;
					ConstructTreeNodes(treeNodes, GroupPage.Description.ToString());
				}
            }
            else
            {
                if (!string.IsNullOrEmpty(GroupPageName) &&
                    GroupPageType != DynamicOptionType.BusinessHierarchy)
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        uow.Reassociate(_groupPageHelper.PersonCollection);

                        TreeConstructor treeConstructor = new TreeConstructor(_groupPageHelper);
                        GroupPage = treeConstructor.CreateGroupPage(GroupPageName, GroupPageType, OptionalColumnId);
                    }

                    if (GroupPage != null)
                    {
                        //tabControlAdvPreviewTab.TabPages[0].Tag = GroupPage;

                        //retrieve tree nodes.
                        IList<TreeNodeAdv> treeNodes = _treeViewCreator.CreateView(GroupPage, _myApplicationFunction);

                        ConstructTreeNodes(treeNodes, GroupPage.Description.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can drop the specified source node.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="destinationNode">The destination node.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can drop the specified source node; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private bool CanDrop(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
            if ( // Support drag and drop only within the same tree
                sourceNode.TreeView != treeViewAdvPreviewTree ||
                // Cannot drop into empty area
                destinationNode == null ||
                // Cannot drop over the source's parent
                destinationNode == sourceNode.Parent ||
                // Or over itself
                destinationNode == sourceNode
                )
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether is root selected
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns>
        /// 	<c>true</c> if [is root selected] [the specified nodes]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private static bool IsRootSelected(TreeNodeAdv[] nodes)
        {
            IEnumerable<TreeNodeAdv> filteredNodes = System.Linq.Enumerable.Where(
                                                         nodes, n =>
                                                     string.Compare(GroupingConstants.NodeTypeRoot, n.Tag.ToString(),
                                                                    StringComparison.OrdinalIgnoreCase) == 0);

            foreach (TreeNodeAdv node in filteredNodes)
            {
                // This if is not needed 
                if (node != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether is person node
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if [is person node] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private static bool IsPersonNode(TreeNodeAdv node)
        {
            if (node.Tag != null &&
                string.Compare(GroupingConstants.NodeTypePerson, node.Tag.ToString(), StringComparison.OrdinalIgnoreCase) ==
                0)
                return true;

            return false;
        }

        /// <summary>
        /// Creates the new group.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void HandleCreateNewGroup()
        {
            using (NameDialog nameDialog = new NameDialog("xxEnterNewGroupName", "xxNewGroupName", ""))
            {
                nameDialog.Text = UserTexts.Resources.NewGroupName;
                if (nameDialog.ShowDialog(this) == DialogResult.OK)
                {
                    IRootPersonGroup rootPersonGroup = new RootPersonGroup(nameDialog.NameValue);
                    GroupPage.AddRootPersonGroup(rootPersonGroup);

                    TreeNodeAdv newGroupNode = new TreeNodeAdv(nameDialog.NameValue);
                    newGroupNode.TagObject = rootPersonGroup;
                    newGroupNode.Tag = GroupingConstants.NodeTypeGroup;
                    newGroupNode.LeftImageIndices = new[] { 2 };

                    treeViewAdvPreviewTree.BeginUpdate();

                    if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
                        treeViewAdvPreviewTree.RowIndexToNode(1).Nodes.Insert(0, newGroupNode);

                    treeViewAdvPreviewTree.EndUpdate();

                    treeViewAdvPreviewTree.CollapseAll();

                    if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
                        treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
                }
            }
        }

        /// <summary>
        /// Changes the name on group.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-04
        /// </remarks>
        private void HandleChangeNameOnGroup()
        {
            if (_rightMouseDownNodeCached != null &&
                string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
                               StringComparison.OrdinalIgnoreCase) == 0)
            {
                treeViewAdvPreviewTree.BeginEdit(_rightMouseDownNodeCached[0]);
            }
        }

        /// <summary>
        /// Sets the unit of work.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-07-06
        /// </remarks>
        public void SetUnitOfWork(IUnitOfWork value)
        {
        }

        /// <summary>
        /// Performs the node move in group page.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="destinationNode">The destination node.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-08
        /// </remarks>
        private void PerformNodeMoveInGroupPage(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
            if (sourceNode != null && sourceNode.TagObject != null &&
                destinationNode != null && destinationNode.TagObject != null)
            {
                if (string.Compare(GroupingConstants.NodeTypePerson, sourceNode.Tag.ToString(),
                                   StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var nodeInDestinationTree = sourceNode;
                    if (findPersonsView1.CutNodes != null)
                    {
                        var selectedNodesCollection = (SelectedNodesCollection)sourceNode.TreeView.SelectedNodes.Clone();
                        var node = selectedNodesCollection[0];
                        for (int i = 0; i < selectedNodesCollection.Count; i++)
                        {
                            if (selectedNodesCollection[i] == sourceNode)
                                node = selectedNodesCollection[i];
                        }
                        nodeInDestinationTree = treeViewAdvPreviewTree.FindNodesWithTagObject(node.TagObject).First();
                    }
                    HandlePersonMove(nodeInDestinationTree, destinationNode);
                }

                if (string.Compare(GroupingConstants.NodeTypeGroup, sourceNode.Tag.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    HandleGroupMove(sourceNode, destinationNode);
                }
            }
        }

        /// <summary>
        /// Handles the person move.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="destinationNode">The destination node.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-08
        /// </remarks>
        private static void HandlePersonMove(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
            IPerson person = sourceNode.TagObject as Person;

            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

                (GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
            {
                //Add this person to the destination group
                AddPerson(person, sourceNode, destinationNode);
            }

            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
               GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.VirtualRoot))
            {
                if (sourceNode.Parent != null)
                {
                    //Remove the person from the source group
                    RemovePerson(sourceNode.Parent, person);
                }
            }

            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
              GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

              (GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
              GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
            {
                if (sourceNode.Parent != null)
                {
                    //Remove the person from the source group
                    RemovePerson(sourceNode.Parent, person);

                    //Add this person to the destination group
                    AddPerson(person, sourceNode, destinationNode);
                }
            }
        }


        private static void AddPerson(IPerson person, TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
            //Add this person to the destination group
            var personRootGroup = destinationNode.TagObject as IRootPersonGroup;
            var personChildGroup = destinationNode.TagObject as IChildPersonGroup;
            var destinationPerson = destinationNode.TagObject as IPerson;

            if (personRootGroup != null)
            {
                personRootGroup.AddPerson(person);
                sourceNode.Move(destinationNode.Nodes);
            }

            if (personChildGroup != null)
            {
                personChildGroup.AddPerson(person);
                sourceNode.Move(destinationNode.Nodes);
            }

            if (destinationPerson != null)
            {
                var parentNode = destinationNode.Parent.TagObject as IChildPersonGroup;
                var parentRootNode = destinationNode.Parent.TagObject as IRootPersonGroup;
                if (parentNode != null)
                    parentNode.AddPerson(person);
                if (parentRootNode != null)
                    parentRootNode.AddPerson(person);
                sourceNode.Move(destinationNode.Parent.Nodes);
            }
        }

        private static void RemovePerson(TreeNodeAdv treeNode, IPerson person)
        {
            //Remove this person from the source group
            IRootPersonGroup personRootGroup = treeNode.TagObject as IRootPersonGroup;
            IChildPersonGroup personChildGroup = treeNode.TagObject as IChildPersonGroup;

            if (personRootGroup != null)
                personRootGroup.RemovePerson(person);

            if (personChildGroup != null)
                personChildGroup.RemovePerson(person);
        }

        /// <summary>
        /// Gets the tree node level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-08
        /// </remarks>
        private static TreeElementLevel GetTreeNodeLevel(int level)
        {
            TreeElementLevel result = TreeElementLevel.Children;

            switch (level)
            {
                case 1:
                    result = TreeElementLevel.VirtualRoot;
                    break;
                case 2:
                    result = TreeElementLevel.Root;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Adds the child members to destinatio node recursively.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="childPersonGroup">The child person group.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-08
        /// </remarks>
        private static void AddChildMembersToDestinatioNodeRecursively(TreeNodeAdv sourceNode, PersonGroupBase childPersonGroup)
        {
            if (sourceNode.HasChildren)
            {
                TreeNodeAdvCollection childNodeCollection = sourceNode.Nodes;

                foreach (TreeNodeAdv node in childNodeCollection)
                {
                    if (string.Compare(GroupingConstants.NodeTypePerson, node.Tag.ToString(),
                                 StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //Add person
                        Person person = node.TagObject as Person;

                        if (person != null)
                            childPersonGroup.AddPerson(person);
                    }
                    else
                    {
                        //Group Node add this
                        ChildPersonGroup childGroup = node.TagObject as ChildPersonGroup;

                        if (childGroup != null)
                        {
                            ChildPersonGroup cPersonGroup = new ChildPersonGroup(childGroup.Description.ToString());
                            childPersonGroup.AddChildGroup(cPersonGroup);
                            AddChildMembersToDestinatioNodeRecursively(node, cPersonGroup);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the group move.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="destinationNode">The destination node.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-08
        /// </remarks>
        private void HandleGroupMove(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
        {
            if (sourceNode.Equals(destinationNode))
                return;
            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

                (GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
            {
                //1
                // Add the source node items to the destination node recursively
                // Remove the source node items from the source node
                IRootPersonGroup sourceGroup = sourceNode.TagObject as IRootPersonGroup;
                PersonGroupBase destinationGroup = destinationNode.TagObject as PersonGroupBase;

                if (sourceGroup != null && destinationGroup != null)
                {
                    GroupPage.RemoveRootPersonGroup(sourceGroup);
                    ChildPersonGroup childPersonGroup = new ChildPersonGroup(sourceGroup.Description.ToString());
                    sourceNode.TagObject = childPersonGroup;
                    //Add other sub tree recursively here
                    AddChildMembersToDestinatioNodeRecursively(sourceNode, childPersonGroup);
                    destinationGroup.AddChildGroup(childPersonGroup);
                }
            }

            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.VirtualRoot))
            {
                //2
                ChildPersonGroup sourceRootPersonGroup = sourceNode.TagObject as ChildPersonGroup;
                PersonGroupBase parentPersonGroupBase = sourceNode.Parent.TagObject as PersonGroupBase;    
 
                if (sourceRootPersonGroup != null && parentPersonGroupBase != null)
                {
                    //remove the group from the parent of the source node
                    parentPersonGroupBase.RemoveChildGroup(sourceRootPersonGroup);

                    //add to root collection
                    RootPersonGroup rootPersonGroup = new RootPersonGroup(sourceRootPersonGroup.Description.ToString());
                    sourceNode.TagObject = rootPersonGroup;
                    //Add other sub tree recursively here
                    AddChildMembersToDestinatioNodeRecursively(sourceNode, rootPersonGroup);
                    GroupPage.AddRootPersonGroup(rootPersonGroup);
                }
            }


            if ((GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

                (GetTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
                GetTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
            {
                //3
                //remove from it's parent
                // add to the node
                ChildPersonGroup sourceGroup = sourceNode.TagObject as ChildPersonGroup;
                PersonGroupBase destinationGroup = destinationNode.TagObject as PersonGroupBase;
                PersonGroupBase parentPersonGroupBase = sourceNode.Parent.TagObject as PersonGroupBase;

                if (sourceGroup != null && destinationGroup != null && parentPersonGroupBase != null)
                {
                    parentPersonGroupBase.RemoveChildGroup(sourceGroup);
                    ChildPersonGroup childPersonGroup = new ChildPersonGroup(sourceGroup.Description.ToString());
                    sourceNode.TagObject = childPersonGroup;
                    AddChildMembersToDestinatioNodeRecursively(sourceNode, childPersonGroup);
                    destinationGroup.AddChildGroup(childPersonGroup);
                }
            }

            if (_cutSourceNode != null)
            {
                for (var i = 0; i < _cutSourceNode.Count; i++)
                {
                    _cutSourceNode[i].Move(_rightMouseDownNodeCached[0].Nodes, i);
                }
            }
        }

        private void ConstructTreeNodes(IList<TreeNodeAdv> treeNodes, string rootName)
        {
            treeViewAdvPreviewTree.BeginUpdate();

            TreeNodeAdv virtualNode = new TreeNodeAdv(rootName);
            virtualNode.TagObject = "RootGroup";
            virtualNode.Tag = GroupingConstants.NodeTypeRoot;
            virtualNode.LeftImageIndices = new[] { 1 };
            virtualNode.RightImagePadding = 10;

            foreach (TreeNodeAdv node in treeNodes)
                virtualNode.Nodes.Add(node);

            treeViewAdvPreviewTree.Nodes.Add(virtualNode);
            treeViewAdvPreviewTree.EndUpdate();

            if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
                treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
        }

        private void PerformPersonDeletion(TreeNodeAdv selectedNode)
        {
            if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children)
            {
                var person = selectedNode.TagObject as Person;

                if (person != null)
                {
                    var parentGroup = selectedNode.Parent.TagObject as PersonGroupBase;

                    if (parentGroup != null)
                    {
                        //Confirm before delete
                        DialogResult response =
                                ShowYesNoMessage(
                                    string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.PersonFromGroupingDeletionQuestion,
                                                 person.Name), UserTexts.Resources.ConfirmDeletion);

                        if (response == DialogResult.Yes)
                        {
                            //Remove the person from the node and assign to the virtual root node
                            parentGroup.RemovePerson(person);

                            if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
                                selectedNode.Move(treeViewAdvPreviewTree.RowIndexToNode(1).Nodes);
                        }
                    }
                }
            }
        }

        private static bool CanPerformPersonDeletion(TreeNodeAdv selectedNode)
        {
            if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children)
            {
                var person = selectedNode.TagObject as Person;

                if (person != null)
                {
                    var parentGroup = selectedNode.Parent.TagObject as PersonGroupBase;

                    if (parentGroup != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        private void PerformGroupDeletion(TreeNodeAdv selectedNode)
        {
            if (selectedNode.HasChildren)
            {
                ViewBase.ShowWarningMessage(UserTexts.Resources.TheGroupHasToBeEmptyBeforeDeleted, UserTexts.Resources.GroupIsNotEmpty);
            }
            else
            {
                if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Root)
                {
                    var sourceGroup = selectedNode.TagObject as IRootPersonGroup;
                    if (sourceGroup != null)
                    {
                        GroupPage.RemoveRootPersonGroup(sourceGroup);
                        selectedNode.Remove();
                        return;
                    }
                }

                if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children &&
                    selectedNode.Parent != null)
                {
                    var sourceGroup = selectedNode.TagObject as IChildPersonGroup;
                    var parentRootPersonGroup = selectedNode.Parent.TagObject as IRootPersonGroup;
                    var parentChildPersonGroup = selectedNode.Parent.TagObject as IChildPersonGroup;

                    if (parentRootPersonGroup != null)
                    {
                        parentRootPersonGroup.RemoveChildGroup(sourceGroup);
                        selectedNode.Remove();
                    }
                    if (parentChildPersonGroup != null)
                    {
                        parentChildPersonGroup.RemoveChildGroup(sourceGroup);
                        selectedNode.Remove();
                    }
                }
            }
        }

        private static bool CanPerformGroupDeletion(TreeNodeAdv selectedNode)
        {
            if (selectedNode.HasChildren)
            {
                return true;
            }

            if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Root)
            {
                return true;
            }

            if (GetTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children &&
                selectedNode.Parent != null)
            {
                var parentRootPersonGroup = selectedNode.Parent.TagObject as IRootPersonGroup;
                var parentChildPersonGroup = selectedNode.Parent.TagObject as IChildPersonGroup;

                if (parentRootPersonGroup != null)
                {
                    return true;
                }
                if (parentChildPersonGroup != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void handleDeletion()
        {
            if (_rightMouseDownNodeCached != null)
            {
                if (string.Compare(GroupingConstants.NodeTypePerson, _rightMouseDownNodeCached[0].Tag.ToString(),
                              StringComparison.OrdinalIgnoreCase) == 0)
                {
                    PerformPersonDeletion(_rightMouseDownNodeCached[0]);
                }
                else if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
                              StringComparison.OrdinalIgnoreCase) == 0)
                {
                    PerformGroupDeletion(_rightMouseDownNodeCached[0]);
                }
            }
        }

        private void handleCollapseAllNodes()
        {
            if (treeViewAdvPreviewTree.RowIndexToNode(1).Expanded)
                treeViewAdvPreviewTree.CollapseAll();
        }

        private void handleExpandOnlyRootNodes()
        {
            if (treeViewAdvPreviewTree.RowIndexToNode(1).Expanded)
                treeViewAdvPreviewTree.CollapseAll();

            if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
                treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
        }

        private void handleExpandAllNodes()
        {
            treeViewAdvPreviewTree.ExpandAll();
        }

        private void handleCut()
        {
            if (_rightMouseDownNodeCached != null)
            {
                _cutSourceNode = (SelectedNodesCollection) _rightMouseDownNodeCached.Clone();
            }
        }

        private void handlePaste()
        {
            if (findPersonsView1.CutNodes != null)
                _cutSourceNode = findPersonsView1.CutNodes;

            if (_rightMouseDownNodeCached != null)
            {
                if (_cutSourceNode != null)
                {
                    for (var i = 0; i < _cutSourceNode.Count; i++)
                    {
	                    if (!IsPersonNode(_cutSourceNode[i]))
	                    {
							if(_cutSourceNode[i].HasNode(_rightMouseDownNodeCached[0]) || _cutSourceNode[i] == _rightMouseDownNodeCached[0])
   								continue;
	                    }
                        PerformNodeMoveInGroupPage(_cutSourceNode[i], _rightMouseDownNodeCached[0]);   
                    }
                    _rightMouseDownNodeCached[0].Expand();
                    _cutSourceNode = null;
                    findPersonsView1.ClearNodes();
                }
            }
        }


        private void treeViewAdvPreviewTreeKeyDown(object sender, KeyEventArgs e)
        {
             if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
             {
                 _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
                 handleCut();
             }

             if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
             {
                 _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
                 handlePaste();
             }

             base.OnKeyDown(e);
        }
    }
}
