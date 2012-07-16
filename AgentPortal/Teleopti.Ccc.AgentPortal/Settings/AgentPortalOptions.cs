using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.Settings
{
    /// <summary>
    /// Represent the Common contain form for all setting related User control
    /// in Agent Portal
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-08-18
    /// </remarks>
    public partial class AgentPortalOptions : BaseRibbonForm
    {
    	/// <summary>
        /// holds refrence list of all types of setting controls
        /// </summary>
        private readonly IList<IDialogControl> _controlList = new List<IDialogControl>();

        /// <summary>
        /// hols reference to a list seeting controls that has been initialized
        /// </summary>
        private readonly List<int> _initializedControls = new List<int>();

    	/// <summary>
        /// Sets the common texts.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-04-08
        /// </remarks>
        protected override void SetCommonTexts()
        {
            buttonAdvOK.Text = UserTexts.Resources.Ok;
            buttonAdvCancel.Text = UserTexts.Resources.Cancel;
        }

    	/// <summary>
        /// Initializes a new instance of the <see cref="AgentPortalOptions"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        public AgentPortalOptions()
        {
            InitializeComponent();

            if (!this.DesignMode) SetTexts();
            InitializeOptionDialog();
        }

    	/// <summary>
        /// Handles the AfterSelect event of the treeViewOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        void treeViewOptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            panelControlHolder.Controls.Clear();
            IDialogControl item = (IDialogControl)e.Node.Tag;
            int controlIndex = _controlList.IndexOf(item);
            if (!_initializedControls.Contains(controlIndex))
            {
                _initializedControls.Add(controlIndex);
                item.InitializeDialogControl();
                item.LoadControl();
            }

            panelControlHolder.Controls.Add((Control)item);
            panelControlHolder.Controls[0].Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/3/2008
        /// </remarks>
        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the buttonAdvOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 4/3/2008
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortal.Helper.MessageBoxHelper.ShowErrorMessage(System.String,System.String)")]
		private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (SaveChanges())
                {
                    Close();
                }
            }
            catch (PasswordMismatchException argExp)
            {
                MessageBoxHelper.ShowWarningMessage(argExp.Message, UserTexts.Resources.AgentPortal);
            }
            catch (DataException ex)
            {
                string dataErrorMessage = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.ErrorOccuredWhenAccessingTheDataSource + ".\n\nError information: {0}", ex.Message);
                MessageBoxHelper.ShowErrorMessage(dataErrorMessage, UserTexts.Resources.AgentPortal);
            }
        }

    	/// <summary>
        /// Initializes the option dialog.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        private void InitializeOptionDialog()
        {
            treeViewOptions.AfterSelect += treeViewOptions_AfterSelect;
            _controlList.Add(new ChangeCultureControl());
            _controlList.Add(new ChangePasswordControl());
            LoadTree();
        }

        /// <summary>
        /// Loads the tree.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        private void LoadTree()
        {
            foreach (IDialogControl item in _controlList)
            {
                TreeNode parent = GetTreeNode(item.TreeFamily());
                if (parent == null)
                {
                    parent = treeViewOptions.Nodes.Add(item.TreeFamily());
                    parent.Tag = item;
                }
                TreeNode node = parent.Nodes.Add(item.TreeNode());
                node.Tag = item;
            }
            treeViewOptions.SelectedNode = treeViewOptions.Nodes[0];
            treeViewOptions.SelectedNode.Expand();

        }

        /// <summary>
        /// Gets the tree node.
        /// </summary>
        /// <param name="NodeName">Name of the node.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        private TreeNode GetTreeNode(string NodeName)
        {
            foreach (TreeNode item in treeViewOptions.Nodes)
            {
                if (item.Text == NodeName && item.Parent == null)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-18
        /// </remarks>
        private bool SaveChanges()
        {
            bool result = true;
            foreach (IDialogControl item in _controlList)
            {
                int idx = _controlList.IndexOf(item);
                if (_initializedControls.Contains(idx))
                    if (!item.SaveChanges())
                        result = false;
            }
            return result;
        }
    }
}

