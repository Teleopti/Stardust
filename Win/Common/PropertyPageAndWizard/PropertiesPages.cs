using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
    public partial class PropertiesPages : BaseDialogForm
    {
		private readonly IAbstractPropertyPages _propertyPages;

        protected PropertiesPages()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColor();
        }

        private void SetColor()
        {
            splitContainerHorizontal.BackColor = ColorHelper.WizardPanelSeparator();
            splitContainerPages.BackColor = ColorHelper.WizardPanelSeparator();
            splitContainerHorizontal.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
            splitContainerHorizontal.Panel2.BackColor = ColorHelper.WizardPanelButtonHolder();
            splitContainerPages.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
            splitContainerPages.Panel2.BackColor = ColorHelper.WizardPanelBackgroundColor();
            treeViewPages.BackColor = ColorHelper.StandardTreeBackgroundColor();
        }

        public PropertiesPages(IAbstractPropertyPages propertyPages) : this()
        {
			Name = Name + "." + propertyPages.GetType().Name; // For TestComplete
            _propertyPages = propertyPages;
            _propertyPages.Owner = this;
            if (!_propertyPages.ModeCreateNew)
            {
                _propertyPages.LoadAggregateRootWorkingCopy();
            }
        }

        private void PropertiesPages_Load(object sender, EventArgs e)
        {
            if (_propertyPages != null)
            {
                treeViewPages.BeginUpdate();
                treeViewPages.Nodes.Clear();
                foreach (IPropertyPage propertyPage in _propertyPages.Pages)
                {
                    TreeNode node = new TreeNode(propertyPage.PageName);
                    node.Name = propertyPage.PageName;
                    node.Tag = propertyPage;
                    treeViewPages.Nodes.Add(node);
                }
                treeViewPages.ExpandAll();
                treeViewPages.EndUpdate();
                displayPage(_propertyPages.FirstPage);
            }
        }

    	private void displayPage(IPropertyPage pp)
        {
            pp = _propertyPages.ShowPage(pp);
            if (!_propertyPages.ModeCreateNew)
                pp.SetEditMode();

            SuspendLayout();
            splitContainerPages.SuspendLayout();
            Control c = (Control)pp;
            splitContainerPages.Panel2.Controls.Clear();
            c.Dock = DockStyle.Fill;
            c.TabIndex = 1;
    		splitContainerPages.Panel2.Controls.Add(c);
            foreach (TreeNode treeNode in treeViewPages.Nodes)
            {
                treeNode.BackColor = Color.Empty;
            }
            treeViewPages.Nodes[_propertyPages.CurrentPage.PageName].BackColor = ColorHelper.StandardTreeSelectedItemColor();//Color.DodgerBlue;
            splitContainerPages.ResumeLayout();
            ResumeLayout();

			c.Select();
            _propertyPages.NameChanged += pp_NameChanged;

        }

        private void pp_NameChanged(object sender, WizardNameChangedEventArgs e)
        {
            Text = e.NewName;
        }


        private void treeViewPages_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IPropertyPage pp = (IPropertyPage) e.Node.Tag;
            displayPage(pp);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            IEnumerable<IRootChangeInfo> updatesMade = _propertyPages.Save();
            if (updatesMade != null)
            {
                Close();
                Main.EntityEventAggregator.TriggerEntitiesNeedRefresh(this, updatesMade);
                DialogResult = DialogResult.OK;
            }
            Cursor = Cursors.Default;
        }
    }
}
