using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
	public partial class Wizard : BaseDialogForm
	{
		private readonly IAbstractPropertyPages _propertyPages;
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();
		private TreeNodeAdv _rootNode;

		protected Wizard()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			SetColor();
		}

		private void SetColor()
		{
			//BackColor = ColorHelper.StandardPanelBackground();
			//treeViewPages.BackColor = ColorHelper.StandardTreeBackgroundColor();
			//splitContainerHorizontal.BackColor = ColorHelper.WizardPanelSeparator();
			//splitContainerPages.BackColor = ColorHelper.WizardPanelSeparator();
			//splitContainerHorizontal.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
			//splitContainerHorizontal.Panel2.BackColor = ColorHelper.WizardPanelButtonHolder();
			//splitContainerPages.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
			//splitContainerPages.Panel2.BackColor = ColorHelper.WizardPanelBackgroundColor();
			labelHeading.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		public Wizard(IAbstractPropertyPages propertyPages)
			: this()
		{
			Name = Name + "." + propertyPages.GetType().Name; // For TestComplete
			_propertyPages = propertyPages;
			_propertyPages.Owner = this;
			_propertyPages.NameChanged += ppNameChanged;
			if (!_propertyPages.ModeCreateNew)
			{
				_propertyPages.LoadAggregateRootWorkingCopy();
			}
			setWindowText();
		}

		private void setWindowText()
		{
			Text = _propertyPages.WindowText;
		}

		private void buttonBackClick(object sender, EventArgs e)
		{
			displayPage(_propertyPages.PreviousPage());
		}

		private void buttonNextClick(object sender, EventArgs e)
		{
			displayPage(_propertyPages.NextPage());
		}

		private void buttonCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void buttonFinishClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var isSucceeded = new GracefulDataSourceExceptionHandler().AttemptDatabaseConnectionDependentAction(() =>
			{
				_propertyPages.AddToRepository();
				IEnumerable<IRootChangeInfo> updatesMade = _propertyPages.Save();
				if (updatesMade != null)
				{
					Close();
					Main.EntityEventAggregator.TriggerEntitiesNeedRefresh(this, updatesMade);
					DialogResult = DialogResult.OK;
				}
			});
			if (!isSucceeded) Close();
			Cursor = Cursors.Default;
		}

		private void propertyPageWizardLoad(object sender, EventArgs e)
		{
			//treeViewPages.RightToLeftLayout = RightToLeftLayout;
			if (_propertyPages != null)
			{
				buildTreeView();
			}
		}

		private void buildTreeView()
		{
			MinimumSize = _propertyPages.MinimumSize;
			treeViewPages.BeginUpdate();
			treeViewPages.Nodes.Clear();
			_rootNode = new TreeNodeAdv(_propertyPages.Name);
			treeViewPages.Nodes.Add(_rootNode);
			string[] pageNames = _propertyPages.GetPageNames();
			foreach (string pageName in pageNames)
			{
				var tempNode = new TreeNodeAdv(pageName);
				_rootNode.Nodes.Add(tempNode);
			}
			treeViewPages.ExpandAll();
			treeViewPages.EndUpdate();
		}

		private void displayPage(IPropertyPage pp)
		{
			if (!_propertyPages.ModeCreateNew)
				pp.SetEditMode();

			SuspendLayout();
			splitContainerVertical.SuspendLayout();
			var control = buildControlFromPropertyPage(pp);
			setButtonState();
			adjustFormSize(control);
			treeViewPages.SelectedNodes.Clear();
			foreach (TreeNodeAdv treeNode in _rootNode.Nodes)
			{
				if (treeNode.Text == _propertyPages.CurrentPage.PageName)
					treeViewPages.SelectedNodes.Add(treeNode);
			}
			splitContainerVertical.ResumeLayout();
			ResumeLayout();

			control.Focus();
		}

		private Control buildControlFromPropertyPage(IPropertyPage pp)
		{
			var c = (Control)pp;
			splitContainerVertical.Panel2.Controls.Clear();
			c.Dock = DockStyle.Fill;
			labelHeading.Text = pp.PageName;
			splitContainerVertical.Panel2.Controls.Add(c);
			return c;
		}

		private void adjustFormSize(Control control)
		{
			var currentFormHeight = Height;
			var currentFormWidth = Width;
			var currentControlSize = getControlSize(control);
			const int heightOffset = 50;
			const int widthOffset = 180;
			if (currentControlSize.Height + heightOffset > currentFormHeight)
				Height = currentControlSize.Height + heightOffset;
			if (currentControlSize.Width + widthOffset > currentFormWidth)
				Width = currentControlSize.Width + widthOffset;
		}

		private Size getControlSize(Control control)
		{
			var savedAnchor = control.Anchor;
			var saveDock = control.Dock;
			control.Anchor =
				((((AnchorStyles.Top
					| AnchorStyles.Bottom)
				   | AnchorStyles.Left)
				  | AnchorStyles.Right)); // needed to be able to get the dynamic control's size

			var currentControlHeight = control.Height;
			var currentControlWidth = control.Width;

			control.Anchor = savedAnchor; //restore original state
			control.Dock = saveDock;

			return new Size(currentControlWidth, currentControlHeight);
		}

		private void setButtonState()
		{
			buttonBack.Enabled = !_propertyPages.IsOnFirst();
			buttonNext.Enabled = !_propertyPages.IsOnLast();
			buttonFinish.Enabled = _propertyPages.IsOnLast();
			if (_propertyPages.IsOnLast())
			{
				AcceptButton = buttonFinish;
			}
			else
			{
				AcceptButton = buttonNext;
			}
		}

		private void ppNameChanged(object sender, WizardNameChangedEventArgs e)
		{
			treeViewPages.Nodes[0].Text = e.NewName;
		}

		private void splitContainerPagesDoubleClick(object sender, EventArgs e)
		{
			splitContainerPages.Panel1Collapsed = !splitContainerPages.Panel1Collapsed;
		}

		private void splitContainerPagesPaint(object sender, PaintEventArgs e)
		{
			using (var brush = new SolidBrush(Color.FromKnownColor(KnownColor.ActiveBorder)))
			{
				e.Graphics.FillRectangle(brush, e.ClipRectangle);
			}
		}

		private void wizardFormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void wizardShown(object sender, EventArgs e)
		{
			if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(displayFirstPage))
			{
				Close();
			}
		}

		private void displayFirstPage()
		{
			displayPage(_propertyPages.ShowPage(_propertyPages.FirstPage));
		}

		private void treeViewPagesBeforeSelect(object sender, TreeViewAdvCancelableSelectionEventArgs args)
		{
			args.Cancel = true;
		}

		private void treeViewPagesBeforeCollapse(object sender, TreeViewAdvCancelableNodeEventArgs e)
		{
			e.Cancel = true;
		}

		private void splitContainerVertical_Panel2_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}