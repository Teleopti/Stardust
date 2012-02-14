using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll
{
    public partial class DefinitionSetNavigationView : PayrollBaseUserControl, 
                                                       IDefinitionSetView
    {
        #region Fields - Instance Members

        #endregion

        #region Properties - Instance Members

        #endregion
        
        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionSetNavigationView"/> class.
        /// </summary>
        /// <param name="explorerView">The explorer view.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        public DefinitionSetNavigationView(IExplorerView explorerView)
            : base(explorerView)
        {
            InitializeComponent();
            tvDefinitionSets.NodeEditorValidated += tvDefinitionSets_NodeEditorValidated;
            tvDefinitionSets.AfterSelect += tvDefinitionSets_AfterSelect;
            PrepareContextMenuStrip();
            LoadDefinitionSets();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AfterSelect event of the tvDefinitionSets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void tvDefinitionSets_AfterSelect(object sender, EventArgs e)
        {
            int selectedNodeCount = (tvDefinitionSets.SelectedNodes.Count);
            IList<IMultiplicatorDefinitionSet> filteredSet = new List<IMultiplicatorDefinitionSet>(tvDefinitionSets.SelectedNodes.Count);
            for (int i = 0; i <= (selectedNodeCount - 1); i++)
            {
                TreeNodeAdv node = tvDefinitionSets.SelectedNodes[i];
                IMultiplicatorDefinitionSet tag = node.TagObject as IMultiplicatorDefinitionSet;
                if (tag != null)
                    filteredSet.Add(tag);
            }

            ExplorerView.ExplorerPresenter.Model.FilteredDefinitionSetCollection = filteredSet;
            ExplorerView.RefreshSelectedViews();
        }

        /// <summary>
        /// Handles the NodeEditorValidated event of the tvDefinitionSets control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Tools.TreeNodeAdvEditEventArgs"/> instance containing the event data.</param>
        private void tvDefinitionSets_NodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
        {
            if (tvDefinitionSets.SelectedNode != null)
            {
                if (tvDefinitionSets.SelectedNodes.Count == 1)
                {
                    TreeNodeAdv currentNode = tvDefinitionSets.SelectedNodes[0];
                    IMultiplicatorDefinitionSet definitionSet = currentNode.TagObject as IMultiplicatorDefinitionSet;
                    ExplorerView.ExplorerPresenter.DefinitionSetPresenter.RenameDefinitionSet(definitionSet, currentNode.Text);
                }
            }
        }

        /// <summary>
        /// Handles the DefinitionSetAddedEventArgs event of the EditForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EditForm_AfterDefinitionSetAdded(object sender, DefinitionSetAddedEventArgs e)
        {
            AddNode(CreateNode(e.DefinitionSetViewModel));
        }

        private void tvDefinitionSets_Click(object sender, EventArgs e)
        {
            ExplorerView.SetSelectedView(PayrollViewType.DefinitionSetDropDown);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepares the context menu strip.
        /// </summary>
        private void PrepareContextMenuStrip()
        {
            tvDefinitionSets.ContextMenuStrip = new ContextMenuStrip();
            
            ToolStripMenuItem tsiNewDefinitionSet = new ToolStripMenuItem(Resources.New);
            tsiNewDefinitionSet.Click += delegate { AddNew(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiNewDefinitionSet);

            ToolStripMenuItem tsiRenameDefinitionSet = new ToolStripMenuItem(Resources.Rename);
            tsiRenameDefinitionSet.Click += delegate { Rename(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiRenameDefinitionSet);

            ToolStripMenuItem tsiDeleteDefinitionSet = new ToolStripMenuItem(Resources.Delete);
            tsiDeleteDefinitionSet.Click += delegate { DeleteSelected(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiDeleteDefinitionSet);

            ToolStripMenuItem tsiCutDefinitionSet = new ToolStripMenuItem(Resources.Cut);
            tsiCutDefinitionSet.Click += delegate { Cut(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiCutDefinitionSet);

            ToolStripMenuItem tsiCopyDefinitionSet = new ToolStripMenuItem(Resources.Copy);
            tsiCopyDefinitionSet.Click += delegate { Copy(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiCopyDefinitionSet);

            ToolStripMenuItem tsiPasteDefinitionSet = new ToolStripMenuItem(Resources.Paste);
            tsiPasteDefinitionSet.Click += delegate { Paste(); };
            tvDefinitionSets.ContextMenuStrip.Items.Add(tsiPasteDefinitionSet);
        }

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="node">The node.</param>
        private void AddNode(TreeNodeAdv node)
        {
            tvDefinitionSets.Nodes.Add(node);
        }

        /// <summary>
        /// Creates the node.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private static TreeNodeAdv CreateNode(IViewModel<IMultiplicatorDefinitionSet> item)
        {
            TreeNodeAdv node = new TreeNodeAdv() { Text = item.DomainEntity.Name, TagObject = item.DomainEntity };
            return node;
        }

        /// <summary>
        /// Removes the selected definition sets.
        /// </summary>
        private void RemoveSelectedDefinitionSets()
        {
            int selectedNodeCount = (tvDefinitionSets.SelectedNodes.Count - 1);
            for (int i = selectedNodeCount; i >= 0; i--)
            {
                TreeNodeAdv currentNode = tvDefinitionSets.SelectedNodes[i];
                IMultiplicatorDefinitionSet definitionSet = currentNode.TagObject as IMultiplicatorDefinitionSet;
                ExplorerView.ExplorerPresenter.DefinitionSetPresenter.RemoveDefinitionSet(definitionSet);
                tvDefinitionSets.Nodes.Remove(currentNode);
            }
        }

        #endregion

        #region Methods - Overriden Methods

        /// <summary>
        /// Adds the new.
        /// </summary>
        public override void AddNew()
        {
            ManageDefinitionSetForm editForm = new ManageDefinitionSetForm(ExplorerView.ExplorerPresenter.DefinitionSetPresenter);
            editForm.DefinitionSetAddedEventArgs += EditForm_AfterDefinitionSetAdded;
            editForm.ShowDialog();
        }

        /// <summary>
        /// Deletes the selected items.
        /// </summary>
        public override void DeleteSelected()
        {
            int count = tvDefinitionSets.SelectedNodes.Count;
            if (count == 0)
            {
                MessageBoxAdv.Show(
                    string.Concat(Resources.SelectRuleSetMessage, "  "),
                                Text,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0
                    );
                return;
            }

            if (!ExplorerView.CheckForDelete())
                return;

            RemoveSelectedDefinitionSets();
        }

        /// <summary>
        /// Renames this instance.
        /// </summary>
        public override void Rename()
        {
            if (tvDefinitionSets.SelectedNode != null)
            {
                if (tvDefinitionSets.SelectedNodes.Count == 1)
                {
                    tvDefinitionSets.BeginEdit(tvDefinitionSets.SelectedNodes[0]);
                }
            }
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        /// <param name="mode"></param>
        public override void Sort(SortingMode mode)
        {
            tvDefinitionSets.Nodes.Sort((mode == SortingMode.Ascending) ? SortOrder.Ascending : SortOrder.Descending);
        }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        public override void Cut()
        {
            ClipboardHandler.Instance.CopySelection(tvDefinitionSets);
            RemoveSelectedDefinitionSets();
            tvDefinitionSets.SelectedNodes.Clear();
            ExplorerView.SetClipboardControlState(ClipboardOperation.Paste, true);
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public override void Copy()
        {
            ClipboardHandler.Instance.CopySelection(tvDefinitionSets);
            ExplorerView.SetClipboardControlState(ClipboardOperation.Paste, true);
        }

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public override void Paste()
        {
            tvDefinitionSets.BeginUpdate();
            foreach (ICloneable entity in ClipboardHandler.Instance.Clips)
            {
                IMultiplicatorDefinitionSet realEntity = entity as IMultiplicatorDefinitionSet;
                if (realEntity != null)
                {
                    IMultiplicatorDefinitionSet clonnedEntity = realEntity.NoneEntityClone();
                    ExplorerView.ExplorerPresenter.DefinitionSetPresenter.AddNewDefinitionSet(clonnedEntity);
                    TreeNodeAdv copiedNode = new TreeNodeAdv()
                    {
                        Text = clonnedEntity.Name, 
                        TagObject = clonnedEntity
                    };
                    tvDefinitionSets.Nodes.Add(copiedNode);
                }
            }
            tvDefinitionSets.EndUpdate();
            if(ExplorerView.ClipboardActionType == ClipboardOperation.Cut)
            {
                ClipboardHandler.Instance.Clips.Clear();
                ExplorerView.SetClipboardControlState(ClipboardOperation.Paste, false);
            }
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public override void Reload()
        {
            
        }

        #endregion

        #region IDefinitionSetView Members

        /// <summary>
        /// Loads the definition sets.
        /// </summary>
        public void LoadDefinitionSets()
        {
            if (ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection != null &&
                ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection.Count > 0)
            {
                tvDefinitionSets.Nodes.Clear();
                tvDefinitionSets.BeginUpdate();
                foreach (IDefinitionSetViewModel definitionSet in ExplorerView.ExplorerPresenter.DefinitionSetPresenter.ModelCollection)
                {
                    AddNode(CreateNode(definitionSet));
                }
                tvDefinitionSets.EndUpdate();
            }
        }

        #endregion
    }
}
