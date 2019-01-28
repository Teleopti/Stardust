using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard
{

	public partial class WizardNoRoot<T> : BaseDialogForm where T : class
	{
		private readonly IAbstractPropertyPagesNoRoot<T> _propertyPages;
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();
		private TreeNodeAdv _rootNode;
		private readonly IComponentContext _container;
		private readonly IToggleManager _toggleManager;

		protected WizardNoRoot()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			setColor();
		}

		private void setColor()
		{
		labelHeading.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		public WizardNoRoot(IAbstractPropertyPagesNoRoot<T> propertyPages, IComponentContext container)
			: this()
		{
			_container = container;
			_toggleManager = _container.Resolve<IToggleManager>();
			Name = Name + "." + propertyPages.GetType().Name; // For TestComplete
			_propertyPages = propertyPages;
			_propertyPages.Owner = this;
			_propertyPages.PageListChanged += onListChanged;

			setWindowText();
		}

		private void onListChanged(object sender, EventArgs e)
		{
			UpdatePageList();
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
			var errorMessageContainer = new ErrorMessageContainer();
			_propertyPages.Pages.ForEach(p => errorMessageContainer.LogError(p.PageName, p.ErrorMessages));
			if (errorMessageContainer.ErrorCount != 0)
			{
				ViewBase.ShowErrorMessage(errorMessageContainer.ToString(), UserTexts.Resources.Message);
				errorMessageContainer.Clear();
				return;
			}
			Cursor = Cursors.WaitCursor;
			var isSucceeded = new GracefulDataSourceExceptionHandler().AttemptDatabaseConnectionDependentAction(() =>
			{
				Close();
				_propertyPages.Save();
				DialogResult = DialogResult.OK;
			});
			if (!isSucceeded) Close();
			Cursor = Cursors.Default;
		}

		private void propertyPageWizardLoad(object sender, EventArgs e)
		{
			UpdatePageList();
		}

		public void UpdatePageList()
		{
			if (_propertyPages != null)
			{
				MinimumSize = _propertyPages.MinimumSize;
				treeViewPages.BeginUpdate();
				treeViewPages.Nodes.Clear();
				_rootNode = new TreeNodeAdv(_propertyPages.Name);
				treeViewPages.Nodes.Add(_rootNode);
				var pageNames = _propertyPages.GetPageNames();
				foreach (var pageName in pageNames)
				{
					var tempNode = new TreeNodeAdv(pageName);
					_rootNode.Nodes.Add(tempNode);
				}
				treeViewPages.ExpandAll();
				treeViewPages.EndUpdate();
			}
		}

		private void displayPage(IPropertyPageNoRoot<T> pp)
		{
			if (!_propertyPages.ModeCreateNew)
				pp.SetEditMode();

			SuspendLayout();
			splitContainerVertical.SuspendLayout();
			var c = (Control)pp;
			splitContainerVertical.Panel2.Controls.Clear();
			c.Dock = DockStyle.Fill;
			labelHeading.Text = pp.PageName;

			if (labelHeading.Text.Length > 15)
			{
				previewPanel.Width = previewPanel.Width - 150;
				previewPanel.Dock = DockStyle.Right;
			}

			splitContainerVertical.Panel2.Controls.Add(c);
			setButtonState();
			if (!_propertyPages.IsOnFirst() ||
				_propertyPages.Name != Resources.QuickForecast ||
				!_toggleManager.IsEnabled(Toggles.WFM_Forecaster_Preview_74801) || 
				!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.WebForecasts))
				previewPanel.Hide();
			treeViewPages.SelectedNodes.Clear();
			foreach (TreeNodeAdv treeNode in _rootNode.Nodes)
			{
				if (treeNode.Text == _propertyPages.CurrentPage.PageName)
					treeViewPages.SelectedNodes.Add(treeNode);
			}
			splitContainerVertical.ResumeLayout();
			ResumeLayout();

			c.Focus();
		}

		private void setButtonState()
		{
			buttonBack.Enabled = !_propertyPages.IsOnFirst();
			buttonNext.Enabled = !_propertyPages.IsOnLast();
			buttonFinish.Enabled = _propertyPages.IsOnLast();
			if (_propertyPages.IsOnLast())
			{
				AcceptButton = buttonFinish;
			} else
			{
				AcceptButton = buttonNext;
			}
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

		private void treeViewPagesBeforeCollapse(object sender, TreeViewAdvCancelableNodeEventArgs e)
		{
			e.Cancel = true;
		}

		private void treeViewPagesBeforeSelect(object sender, TreeViewAdvCancelableSelectionEventArgs args)
		{
			args.Cancel = true;
		}

		private void linkLabelPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(buildWfmUri("WFM/#/forecast").ToString());
		}

		private Uri buildWfmUri(string relativePath)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			return new Uri($"{wfmPath}{relativePath}");
		}
	}

	internal class ErrorMessageContainer
	{
		private readonly IDictionary<string, ICollection<string>> _stepErrors =
			new Dictionary<string, ICollection<string>>();

		public void LogError(string step, ICollection<string> errorMessage)
		{
			_stepErrors.Add(new KeyValuePair<string, ICollection<string>>(step, errorMessage));
		}

		public void Clear()
		{
			_stepErrors.Clear();
		}

		public int ErrorCount
		{
			get { return _stepErrors.Sum(s => s.Value.Count); }
		}

		public override string ToString()
		{
			var error = new StringBuilder();
			foreach (var stepError in _stepErrors)
			{
				var step = stepError;
				stepError.Value.ForEach(v => error.Append(string.Concat(step.Key, UserTexts.Resources.Colon, v, Environment.NewLine)));
			}
			return error.ToString();
		}
	}
}