using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
	public partial class WorkShiftsExplorer : BaseRibbonForm, IExplorerView
	{
		private EditControl _editControl;
		private ClipboardControl _clipboardControl;
		private NavigationView _navigationView;
		private GeneralView _generalView;
		private VisualizeGridView _visualizeView;
		private IDictionary<ShiftCreatorViewType, ToolStripButton> _viewButtonDictionary;
		private readonly IEventAggregator _eventAggregator;
		private readonly IToggleManager _toggleManager;
		private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;
		private readonly IConfigReader _configReader;

		private bool ruleSetBagCopyClicked, ruleSetCopyClicked;

		public WorkShiftsExplorer(IEventAggregator eventAggregator, IToggleManager toggleManager, IBusinessRuleConfigProvider businessRuleConfigProvider, IConfigReader configReader)
		{
			_eventAggregator = eventAggregator;
			_toggleManager = toggleManager;
			_businessRuleConfigProvider = businessRuleConfigProvider;
			_configReader = configReader;

			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				ribbonControlAdv1.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
			}
			setPermissionOnControls();
			ribbonControlAdv1.BeforeContextMenuOpen += ribbonControlBeforeContextMenuOpen;
		}

		public IExplorerPresenter Presenter { get; set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DesignMode) return;
			
			Presenter.Model.SetRightToLeft(RightToLeft == RightToLeft.Yes);
			Presenter.Model.SetSelectedView(ShiftCreatorViewType.RuleSet);

			_navigationView = new NavigationView(Presenter, _eventAggregator);
			_generalView = new GeneralView(Presenter, _eventAggregator);
			_visualizeView = new VisualizeGridView(Presenter, _eventAggregator);

			_navigationView.Refresh += delegate
			{
				_visualizeView.RefreshView();
				var amountList = Presenter.VisualizePresenter.RuleSetAmounts();
				_generalView.Amounts(amountList);
				_generalView.RefreshView();
			};

			_viewButtonDictionary = new Dictionary<ShiftCreatorViewType, ToolStripButton>();
			_viewButtonDictionary.Add(ShiftCreatorViewType.General, toolStripButtonGeneral);
			_viewButtonDictionary.Add(ShiftCreatorViewType.Activities, toolStripButtonCombined);
			_viewButtonDictionary.Add(ShiftCreatorViewType.Limitation, toolStripButtonLimitations);
			_viewButtonDictionary.Add(ShiftCreatorViewType.DateExclusion, toolStripButtonDateExclusion);
			_viewButtonDictionary.Add(ShiftCreatorViewType.WeekdayExclusion, toolStripButtonWeekdayExclusion);

			createAndAddViews();
			instantiateEditControl();
			instantiateClipboardControl();

			toolStripButtonGeneral.Checked = true;
			toolStripButtonAddRuleSet.Enabled = false;

			tsClipboard.Size = tsClipboard.PreferredSize;
			tcEdit.Size = tcEdit.PreferredSize;
			tcShiftBags.Size = tcShiftBags.PreferredSize;
			tcShiftBags.Width = tcShiftBags.Text.Length*8 > tcShiftBags.PreferredSize.Width ? tcShiftBags.Text.Length*8  : tcShiftBags.PreferredSize.Width;
			tcRename.Size = tcRename.PreferredSize;
			tcViews.Size = tcViews.PreferredSize;

			_navigationView.ShowModifyCollection +=
				(sender, args) =>
					{
						if (_navigationView.DefaultTreeView.SelectedNodes == null) return;
						var anythingSelected = _navigationView.DefaultTreeView.SelectedNodes.Count > 0;
						toolStripButtonAddRuleSet.Enabled = anythingSelected;
						_clipboardControl.SetButtonState(ClipboardAction.Paste, anythingSelected && getCurrentViewClickStatus());
						
					};
			KeyDown += workShiftsExplorerKeyDown;
			KeyPress += workShiftsExplorerKeyPress;
		}

		void workShiftsExplorerKeyDown(object sender, KeyEventArgs e)
		{
			if(_navigationView.DefaultTreeView.IsEditing) return;
			if (e.KeyValue.Equals(32))
			{
				e.Handled = true;
			}
		}

		void workShiftsExplorerKeyPress(object sender, KeyPressEventArgs e)
		{
			if (_navigationView.DefaultTreeView.IsEditing) return;
			if (e.KeyChar.Equals((Char)Keys.Space))
			{
				e.Handled = true;
			}
		}

		private void createAndAddViews()
		{
			splitContainerAdvVertical.Panel1.Controls.Add(_navigationView);
			_navigationView.Dock = DockStyle.Fill;
			splitContainerAdvHorizontal.Panel1.Controls.Add(_visualizeView);
			_visualizeView.Dock = DockStyle.Fill;
			splitContainerAdvHorizontal.Panel2.Controls.Add(_generalView);
			_generalView.Dock = DockStyle.Fill;
		}

		private void setPermissionOnControls()
		{
			backStageButton3.Enabled = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
		}

		private void workShiftsExplorerFormClosing(object sender, FormClosingEventArgs e)
		{
			Presenter.Validate();
			if (!Presenter.CheckForUnsavedData())
			{
				e.Cancel = true;
				return;
			}

		    if (_editControl!=null)
		    {
		        _editControl.NewClicked -= editControlNewClicked;
		        _editControl.NewSpecialClicked -= editControlNewSpecialClicked;
		        _editControl.DeleteClicked -= editControlDeleteClicked;
		        _editControl.NewSpecialItems.Clear();
		        _editControl.Dispose();
		        _editControl = null;
		    }
		    KeyDown -= workShiftsExplorerKeyDown;
			KeyPress -= workShiftsExplorerKeyPress;
			ribbonControlAdv1.BeforeContextMenuOpen -= ribbonControlBeforeContextMenuOpen;

			if (_clipboardControl != null)
			{
				_clipboardControl.CopyClicked -= clipboardControlCopyClicked;
				_clipboardControl.PasteClicked -= clipboardControlPasteClicked;
				_clipboardControl = null;
			}

			backStageButton1.Click -= backStageButton1Click;
			backStageButton2.Click -= backStageButton2Click;
			backStageButton3.Click -= backStageButton3Click;
			backStageButton4.Click -= backStageButton4Click;
			backStageButton4.VisibleChanged -= backStageButton4VisibleChanged;
			toolStripButtonSave.Click -= toolStripButtonSaveClick;
			toolStripButtonRefresh.Click -= toolStripButtonRefreshClick;
			toolStripButtonAddRuleSet.Click -= toolStripAssignRuleSetClick;
			toolStripButtonRename.Click -= toolStripButtonRenameClick;
			toolStripButtonGeneral.Click -= toolStripButtonTemplateViewClick;
			toolStripButtonCombined.Click -= toolStripButtonCombinedClick;
			toolStripButtonLimitations.Click -= toolStripButtonLimiterViewClick;
			toolStripButtonDateExclusion.Click -= toolStripButtonDateExclusionClick;
			toolStripButtonWeekdayExclusion.Click -= toolStripButtonWeekdayExclusionClick;

			if (_generalView != null)
			{
				_generalView.Dispose();
				_generalView = null;
			}
			if (_navigationView != null)
			{
				_navigationView.Dispose();
				_navigationView = null;
			}
			if (_visualizeView != null)
			{
				_visualizeView.Dispose();
				_visualizeView = null;
			}

			backStageView1.Dispose();
			backStage1.Dispose();

			_mainWindow?.Activate();
		}

		private void splitContainerAdvHorizontalPanel1Resize(object sender, EventArgs e)
		{
			_visualizeView?.RefreshView();
		}

		private void instantiateEditControl()
		{
			_editControl = new EditControl();
			var editControlHost = new ToolStripControlHost(_editControl);
			tcEdit.Items.Add(editControlHost);

			_editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewRuleSet, Tag = ShiftCreatorViewType.RuleSet });
			_editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewRuleSetBag, Tag = ShiftCreatorViewType.RuleSetBag });
			_editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewActivity, Tag = ShiftCreatorViewType.Activities });
			_editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewLimitation, Tag = ShiftCreatorViewType.Limitation });
			_editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewDateExclusion, Tag = ShiftCreatorViewType.DateExclusion });
			_editControl.NewClicked += editControlNewClicked;
			_editControl.NewSpecialClicked += editControlNewSpecialClicked;

			_editControl.DeleteClicked += editControlDeleteClicked;
			
		}

		private void editControlDeleteClicked(object sender, EventArgs e)
		{
			switch (Presenter.Model.SelectedView)
			{
				case ShiftCreatorViewType.RuleSet:
				case ShiftCreatorViewType.RuleSetBag:
					_navigationView.Delete();
					break;
				case ShiftCreatorViewType.Activities:
				case ShiftCreatorViewType.Limitation:
				case ShiftCreatorViewType.DateExclusion:
				case ShiftCreatorViewType.WeekdayExclusion:
					_generalView.Delete();
					break;
			}
		}

		private void editControlNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			switch((ShiftCreatorViewType) e.ClickedItem.Tag)
			{
				case ShiftCreatorViewType.General: 
				case ShiftCreatorViewType.RuleSet: 
				case ShiftCreatorViewType.RuleSetBag:
					_navigationView.ChangeGridView((ShiftCreatorViewType)e.ClickedItem.Tag);
					_navigationView.Add();
					break;
				case ShiftCreatorViewType.Activities:
				case ShiftCreatorViewType.Limitation:
				case ShiftCreatorViewType.DateExclusion:
					_generalView.ChangeGridView((ShiftCreatorViewType)e.ClickedItem.Tag);
					SelectToolStripItem((ShiftCreatorViewType)e.ClickedItem.Tag);
					_generalView.Add();
					break;
			}
		}

		private void editControlNewClicked(object sender, EventArgs e)
		{
			switch (Presenter.Model.SelectedView)
			{
				case ShiftCreatorViewType.General:
				case ShiftCreatorViewType.RuleSet:
				case ShiftCreatorViewType.RuleSetBag:
					_navigationView.ChangeGridView(Presenter.Model.SelectedView);
					_navigationView.Add();
					break;
				case ShiftCreatorViewType.Activities:
				case ShiftCreatorViewType.Limitation:
				case ShiftCreatorViewType.DateExclusion:
					_generalView.ChangeGridView(Presenter.Model.SelectedView);
					SelectToolStripItem(Presenter.Model.SelectedView);
					_generalView.Add();
					break;
			}
		}

		private void instantiateClipboardControl()
		{
			_clipboardControl = new ClipboardControl();
			var clipboardhost = new ToolStripControlHost(_clipboardControl);
			tsClipboard.Items.Add(clipboardhost);

			_clipboardControl.CopyClicked += clipboardControlCopyClicked;
			_clipboardControl.PasteClicked += clipboardControlPasteClicked;
			_clipboardControl.ToolStripSplitButtonCut.Enabled = false;
			_clipboardControl.ToolStripSplitButtonCut.Visible = false;
			_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
		}

		private void clipboardControlPasteClicked(object sender, EventArgs e)
		{
			switch (Presenter.Model.SelectedView)
			{
				case ShiftCreatorViewType.RuleSet:
				case ShiftCreatorViewType.RuleSetBag:
					_navigationView.Paste();
					break;
				case ShiftCreatorViewType.Activities:
				case ShiftCreatorViewType.Limitation:
				case ShiftCreatorViewType.DateExclusion:
					_generalView.Paste();
					break;
			}
		}

		private void clipboardControlCopyClicked(object sender, EventArgs e)
		{
			_externalExceptionHandler.AttemptToUseExternalResource(Clipboard.Clear);
			switch (Presenter.Model.SelectedView)
			{
				case ShiftCreatorViewType.RuleSet:
				case ShiftCreatorViewType.RuleSetBag:
					_navigationView.Copy();
					if (_navigationView.DefaultTreeView.SelectedNodes.Count > 0)
						_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
					else
						_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
					setClickedStatus();
					break;
				case ShiftCreatorViewType.Activities:
				case ShiftCreatorViewType.Limitation:
				case ShiftCreatorViewType.DateExclusion:
					_generalView.Copy();					
					_clipboardControl.SetButtonState(ClipboardAction.Paste, true);
					break;
			}
		}

		private void setClickedStatus()
		{
			if (_navigationView.CurrentView == ShiftCreatorViewType.RuleSet)
			{
				ruleSetCopyClicked = true;
				ruleSetBagCopyClicked = false;
			}

			if (_navigationView.CurrentView == ShiftCreatorViewType.RuleSetBag)
			{
				ruleSetBagCopyClicked = true;
				ruleSetCopyClicked = false;
			}
		}

		private bool getCurrentViewClickStatus()
		{
			if (_navigationView.CurrentView == ShiftCreatorViewType.RuleSet) return ruleSetCopyClicked;
			if (_navigationView.CurrentView == ShiftCreatorViewType.RuleSetBag) return ruleSetBagCopyClicked;
			return false;
		}

		private void toolStripButtonTemplateViewClick(object sender, EventArgs e)
		{
			loadView(ShiftCreatorViewType.General);
		}

		private void toolStripButtonLimiterViewClick(object sender, EventArgs e)
		{
			loadView(ShiftCreatorViewType.Limitation);
		}

		private void toolStripButtonCombinedClick(object sender, EventArgs e)
		{
			loadView(ShiftCreatorViewType.Activities);
		}

		private DateTime _lastSaveClick;
		private Form _mainWindow;

		private void toolStripButtonSaveClick(object sender, EventArgs e)
		{
			save();
		}

		private void save()
		{
			// fix for bug in syncfusion that shoots click event twice on buttons in quick access
			if (_lastSaveClick.AddSeconds(1) > DateTime.Now) return;
			if (validateGrid())
			{
				Cursor.Current = Cursors.WaitCursor;
				persist();
				toolStripButtonRefresh.PerformClick();
				Cursor.Current = Cursors.Default;
			}
			_lastSaveClick = DateTime.Now;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private bool validateGrid()
		{
			_generalView.ReflectEnteredValues();
			bool status = Presenter.Validate();
			if (!status)
			{
				_generalView.Refresh();
				ShowWarningMessage(UserTexts.Resources.ShiftCreatorValidationErrorMessage,Text);
			}
			return status;
		}

		private void toolStripButtonRefreshClick(object sender, EventArgs e)
		{
			if (validateGrid())
			{
				_navigationView.ForceRefresh();
				_visualizeView.RefreshView();
				var amountList = Presenter.VisualizePresenter.RuleSetAmounts();
				_generalView.Amounts(amountList);
				_generalView.RefreshView();
			}
		}

		private void toolStripAssignRuleSetClick(object sender, EventArgs e)
		{
			_navigationView.AssignRuleSet();
		}

		private void toolStripButtonDateExclusionClick(object sender, EventArgs e)
		{
			loadView(ShiftCreatorViewType.DateExclusion);
		}

		private void toolStripButtonWeekdayExclusionClick(object sender, EventArgs e)
		{
			loadView(ShiftCreatorViewType.WeekdayExclusion);
		}

		private void persist()
		{
			Presenter.Persist();
			
		}

		public void SetupHelpContextForGrid(GridControlBase grid)
		{
			RemoveControlHelpContext(grid);
			AddControlHelpContext(grid);
			grid.Focus();
		}

		public bool AskForDelete()
		{
			DialogResult result = ShowYesNoMessage(UserTexts.Resources.AreYouSureYouWantToDelete, UserTexts.Resources.Delete);
			return (result == DialogResult.Yes);
		}


		public bool CheckForSelectedRuleSet()
		{
			if (Presenter.Model.FilteredRuleSetCollection == null)
			{
				ShowInformationMessage(UserTexts.Resources.YouHaveToSelectAtLeastOneRuleSet,UserTexts.Resources.NoRuleSetSelected);
				return false;
			}
			return true;
		}

		public void SetVisualGridDrawingInfo()
		{
			int widthToAdd = _visualizeView.GetFirstColumnWidth();
			float panelWidth = (float)splitContainerAdvHorizontal.Panel1.Width - splitContainerAdvHorizontal.SplitterWidth - 10;
			float columnCount = Presenter.Model.VisualizeGridColumnCount;

			float widthPerHour = ((panelWidth - widthToAdd) / columnCount);
			float pixelSize = (widthPerHour / 60);

			double width = Presenter.Model.VisualizeGridColumnCount * pixelSize * 60;
			var newWidth = (int)width;
			Presenter.Model.SetVisualColumnWidth(newWidth);
			Presenter.Model.SetWidthPerHour(widthPerHour);
			Presenter.Model.SetWidthPerPixel(pixelSize);
		}

		public void SelectToolStripItem(ShiftCreatorViewType view)
		{
			ToolStripButton selectedButton = null;
			switch(view)
			{
				case ShiftCreatorViewType.General:
					selectedButton = toolStripButtonGeneral;
					break;
				case ShiftCreatorViewType.Activities:
					selectedButton = toolStripButtonCombined;
					break;
				case ShiftCreatorViewType.Limitation:
					selectedButton = toolStripButtonLimitations;
					break;
				case ShiftCreatorViewType.DateExclusion:
					selectedButton = toolStripButtonDateExclusion;
					break;
				case ShiftCreatorViewType.WeekdayExclusion:
					selectedButton = toolStripButtonWeekdayExclusion;
					break;
			}
			if (selectedButton == null) return;
			selectedButton.Checked = true;
			IList<ToolStripButton> buttons = (from p in _viewButtonDictionary where p.Value.Text != selectedButton.Text select p.Value).ToList();
			foreach (ToolStripButton button in buttons)
				button.Checked = false;
		}

		public void RefreshChildViews()
		{
			_navigationView.RefreshView();
			_generalView.RefreshView();
			_visualizeView.RefreshView();
		}

		public void RefreshVisualizeView()
		{
			_visualizeView.RefreshView();
		}

		public void RefreshActivityGridView()
		{
			_generalView.RefreshView();
		}

		public new void AddControlHelpContext(Control control)
		{
			base.AddControlHelpContext(control);
		}

		private void toolStripButtonRenameClick(object sender, EventArgs e)
		{
			_navigationView.Rename();
		}

		private void shiftCreatorKeyUp(object sender, KeyEventArgs e)
		{
			_visualizeView.RefreshView();
		}

		private void loadView(ShiftCreatorViewType view)
		{
			_generalView.ChangeGridView(view);
			Presenter.Model.SetSelectedView(view);
			SelectToolStripItem(view);
		}

		public void ShowDataSourceException(DataSourceException dataSourceException)
		{
			using (var view = new SimpleExceptionHandlerView(dataSourceException,
																   UserTexts.Resources.Shifts,
																	UserTexts.Resources.ServerUnavailable))
			{
				view.ShowDialog();
			}
		}

		public void Show(IExplorerPresenter explorerPresenter, Form mainWindow)
		{
			Presenter = explorerPresenter;
			_mainWindow = mainWindow;
			Show();
		}

		public void ForceRefreshNavigationView()
		{
			_navigationView.ForceRefresh();
		}

		public void UpdateNavigationViewTreeIcons()
		{
			_navigationView.UpdateTreeIcons();
		}

		public void SetViewEnabled(bool enabled)
		{
			Enabled = enabled;
			_generalView.Enabled = enabled;
		}

		public void ExitEditMode()
		{
			validateGrid();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;

			if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
			{
				switch (keyData)
				{
					case Keys.Control | Keys.S:
						toolStripButtonSaveClick(this, EventArgs.Empty);
						break;
					case Keys.Control | Keys.C:
						clipboardControlCopyClicked(this, EventArgs.Empty);
						break;
					case Keys.Control | Keys.V:
						clipboardControlPasteClicked(this, EventArgs.Empty);
						break;
					case Keys.Control | Keys.N:
						editControlNewClicked(this, EventArgs.Empty);
						break;
					case Keys.Control | Keys.R:
					case Keys.F5:
						toolStripButtonRefreshClick(this, EventArgs.Empty);
						break;
					case Keys.F2:
						toolStripButtonRenameClick(this, EventArgs.Empty);
						break;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		static void ribbonControlBeforeContextMenuOpen(object sender, ContextMenuEventArgs e)
		{
			e.Cancel = true;
		}
		private void backStageButton1Click(object sender, EventArgs e)
		{
			save();
		}

		private void backStageButton2Click(object sender, EventArgs e)
		{
			Close();
		}

		private void backStageButton3Click(object sender, EventArgs e)
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager, _businessRuleConfigProvider, _configReader)));
				settings.Show();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void backStageButton4Click(object sender, EventArgs e)
		{
			if (!CloseAllOtherForms(this)) return;

			Close();

			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

		private void backStageButton4VisibleChanged(object sender, EventArgs e)
		{
			backStageButton4.Location = new Point(0, backStageButton4.Location.Y);
		}
	}
}
