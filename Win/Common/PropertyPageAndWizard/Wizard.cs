using System;
using System.Drawing;
using System.Windows.Forms;
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
		private TreeNode _rootNode;

		protected Wizard()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			SetColor();
		}

		private void SetColor()
		{
			BackColor = ColorHelper.StandardPanelBackground();
			treeViewPages.BackColor = ColorHelper.StandardTreeBackgroundColor();
			splitContainerHorizontal.BackColor = ColorHelper.WizardPanelSeparator();
			splitContainerPages.BackColor = ColorHelper.WizardPanelSeparator();
			splitContainerHorizontal.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
			splitContainerHorizontal.Panel2.BackColor = ColorHelper.WizardPanelButtonHolder();
			splitContainerPages.Panel1.BackColor = ColorHelper.WizardPanelBackgroundColor();
			splitContainerPages.Panel2.BackColor = ColorHelper.WizardPanelBackgroundColor();
			gradientPanel1.BackgroundColor = ColorHelper.WizardHeaderBrush;

		}

		public Wizard(IAbstractPropertyPages propertyPages)
			: this()
		{
			Name = Name + "." + propertyPages.GetType().Name; // For TestComplete
			_propertyPages = propertyPages;
			_propertyPages.Owner = this;
			_propertyPages.NameChanged += pp_NameChanged;
			if (!_propertyPages.ModeCreateNew)
			{
				_propertyPages.LoadAggregateRootWorkingCopy();
			}
			SetWindowText();
		}

		private void SetWindowText()
		{
			Text = _propertyPages.WindowText;
		}

		private void buttonBack_Click(object sender, EventArgs e)
		{
			displayPage(_propertyPages.PreviousPage());
		}

		private void buttonNext_Click(object sender, EventArgs e)
		{
			displayPage(_propertyPages.NextPage());
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void buttonFinish_Click(object sender, EventArgs e)
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

		private void PropertyPageWizard_Load(object sender, EventArgs e)
		{
			treeViewPages.RightToLeftLayout = RightToLeftLayout;
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
			_rootNode = treeViewPages.Nodes.Add(_propertyPages.Name);
			string[] pageNames = _propertyPages.GetPageNames();
			foreach (string pageName in pageNames)
			{
				_rootNode.Nodes.Add(pageName, pageName);
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
			foreach (TreeNode treeNode in _rootNode.Nodes)
			{
				treeNode.BackColor = Color.Empty;
			}
			_rootNode.Nodes[_propertyPages.CurrentPage.PageName].BackColor = ColorHelper.StandardTreeSelectedItemColor();
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
			var currentFormHeight = this.Height;
			var currentFormWidth = this.Width;
			var currentControlSize = getControlSize(control);
			const int heightOffset = 50;
			const int widthOffset = 180;
			if (currentControlSize.Height + heightOffset > currentFormHeight)
				this.Height = currentControlSize.Height + heightOffset;
			if (currentControlSize.Width + widthOffset > currentFormWidth)
				this.Width = currentControlSize.Width + widthOffset;
		}

		private Size getControlSize(Control control)
		{
			var savedAnchor = control.Anchor;
			control.Anchor =
				((((AnchorStyles.Top
					| AnchorStyles.Bottom)
				   | AnchorStyles.Left)
				  | AnchorStyles.Right)); // needed to be able to get the dynamic control's size

			var currentControlHeight = control.Height;
			var currentControlWidth = control.Width;

			control.Anchor = savedAnchor; //restore original state

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

		private void pp_NameChanged(object sender, WizardNameChangedEventArgs e)
		{
			treeViewPages.Nodes[0].Text = e.NewName;
		}

		private void treeViewPages_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void treeViewPages_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//e.Node.BackColor = Color.DodgerBlue;
		}

		private void treeViewPages_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void splitContainerPages_DoubleClick(object sender, EventArgs e)
		{
			splitContainerPages.Panel1Collapsed = !splitContainerPages.Panel1Collapsed;
		}


		private void splitContainerPages_Paint(object sender, PaintEventArgs e)
		{
			using (SolidBrush brush = new SolidBrush(Color.FromKnownColor(KnownColor.ActiveBorder)))
			{
				e.Graphics.FillRectangle(brush, e.ClipRectangle);
			}
		}

		private void Wizard_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void Wizard_Shown(object sender, EventArgs e)
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
	}
}