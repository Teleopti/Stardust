using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
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
			labelHeading.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		private void propertiesPagesLoad(object sender, EventArgs e)
		{
			if (_propertyPages != null)
			{
				treeViewPages.BeginUpdate();
				treeViewPages.Nodes.Clear();
				foreach (IPropertyPage propertyPage in _propertyPages.Pages)
				{
					var node = new TreeNodeAdv(propertyPage.PageName) {Tag = propertyPage};
					treeViewPages.Nodes.Add(node);
				}
				treeViewPages.ExpandAll();
				treeViewPages.EndUpdate();
				displayPage(_propertyPages.FirstPage);
				if (treeViewPages.Nodes.Count > 0)
					treeViewPages.SelectedNodes.Add(treeViewPages.Nodes[0]);
			}
		}

		private void displayPage(IPropertyPage pp)
		{
			pp = _propertyPages.ShowPage(pp);
			if (!_propertyPages.ModeCreateNew)
				pp.SetEditMode();

			SuspendLayout();
			panelContainer.SuspendLayout();
			var c = (Control)pp;
			panelContainer.Controls.Clear();
			c.Dock = DockStyle.Fill;
			c.TabIndex = 1;
			panelContainer.Controls.Add(c);
			panelContainer.ResumeLayout();
			ResumeLayout();
			labelHeading.Text = pp.PageName;
		}

		private void buttonOkClick(object sender, EventArgs e)
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

		private void treeViewPagesAfterSelect(object sender, EventArgs e)
		{
			var pp = (IPropertyPage)treeViewPages.SelectedNode.Tag;
			displayPage(pp);
		}

		private void splitContainerPages_SplitterMoved(object sender, SplitterEventArgs e)
		{

		}
	}
}
