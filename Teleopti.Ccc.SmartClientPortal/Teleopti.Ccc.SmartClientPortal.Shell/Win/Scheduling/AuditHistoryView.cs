using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class AuditHistoryView : BaseDialogForm, IAuditHistoryView
	{
		private readonly SchedulingScreen _owner;
		private readonly IAuditHistoryModel _model;
		private readonly AuditHistoryPresenter _presenter;

		public AuditHistoryView(IScheduleDay currentScheduleDay, SchedulingScreen owner)
		{
			_owner = owner;
			InitializeComponent();
			
			if (!DesignMode) SetTexts();

			_model = new AuditHistoryModel(currentScheduleDay, new ScheduleHistoryRepository(UnitOfWorkFactory.CurrentUnitOfWork()), new AuditHistoryScheduleDayCreator());
			_presenter = new AuditHistoryPresenter(this, _model);

			grid.ResetVolatileData();
			grid.QueryCellInfo += gridQueryCellInfo;
			grid.QueryColCount += gridQueryColCount;
			grid.QueryRowCount += gridQueryRowCount;
			grid.QueryColWidth += gridQueryColWidth;
			grid.QueryRowHeight += gridQueryRowHeight;
			grid.SelectionChanging += gridSelectionChanging;
			grid.SelectionChanged += gridSelectionChanged;

			if (!grid.CellModels.ContainsKey("RevisionChangedByCell")) grid.CellModels.Add("RevisionChangedByCell", new RevisionChangedByHeaderCellModel(grid.Model));
			if (!grid.CellModels.ContainsKey("TimeLineHeaderCell"))grid.CellModels.Add("TimeLineHeaderCell", new VisualProjectionColumnHeaderCellModel(grid.Model, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone)); //get from presenter
			if (!grid.CellModels.ContainsKey("RevisionChangeCell")) grid.CellModels.Add("RevisionChangeCell", new RevisionChangeCellModel(grid.Model));
		}

		void gridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (grid.Selections.Count > 1)
			{
				var top = grid.Selections.Ranges[0].Top;
				grid.Selections.Clear();
				grid.Selections.SetSelectClickRowCol(top, 0);
			}

			if (grid.Selections.Ranges.Count > 0 && grid.Selections.Ranges[0].Top != grid.Selections.Ranges[0].Bottom)
			{
				var top = grid.Selections.Ranges[0].Top;
				grid.Selections.Clear();
				grid.Selections.SetSelectClickRowCol(top, 0);
			} 
		}

		static void gridSelectionChanging(object sender, GridSelectionChangingEventArgs e)
		{
			if (e.Reason == GridSelectionReason.MouseMove)
				e.Cancel = true;
	   

			if (!e.Cancel && e.Range.Top == 0 && (e.Range.RangeType == GridRangeInfoType.Cols || e.Range.RangeType == (GridRangeInfoType.Rows | GridRangeInfoType.Cols)))
				e.Cancel = true;   
		}

		void gridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
		{
			var txt = grid[e.Index, 0].FormattedText;
			if (!string.IsNullOrEmpty(txt))
			{
				txt = txt.Substring(0, txt.IndexOf("\n",StringComparison.InvariantCulture));
			}
			var rows = Math.Ceiling(txt.Length/(25.0)) +1;
			e.Size = _presenter.GridQueryRowHeight(e.Index, 28, grid.Font.Height, (int)rows);
			e.Handled = true;
		}

		void gridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			e.Size = _presenter.GridQueryColWidth(e.Index, grid.ClientSize.Width);
			e.Handled = true;
		}

		void gridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryRowCount();
			e.Handled = true;
		}

		void gridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryColCount();
			e.Handled = true;
		}

		void gridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			_presenter.GridQueryCellInfo(sender, e);
			e.Handled = true;
		}

		public void RefreshGrid()
		{
			grid.Refresh();
		}

		public IScheduleDay SelectedScheduleDay
		{
			get { return _model.SelectedScheduleDay; }
		}

		private void buttonCloseClick(object sender, EventArgs e)
		{
			_presenter.Close();
		}

		public void CloseView()
		{
			Close();   
		}

		private void buttonRestoreClick(object sender, EventArgs e)
		{
			IScheduleDay scheduleDay = null;
		   
			if(grid.Selections.Ranges.Count > 0)
			{
				var row = grid.Selections.Ranges[0].Top;
				var col = grid.Selections.Ranges[0].Left;

				var revisionDisplayRow = grid[row, col].CellValue as RevisionDisplayRow;

				if (revisionDisplayRow != null)
					scheduleDay = revisionDisplayRow.ScheduleDay;
			}

			_presenter.Restore(scheduleDay);
		}

		private void auditHistoryViewLoad(object sender, EventArgs e)
		{
			grid.Properties.BackgroundColor = Color.White;
			_presenter.Load();
		}

		public bool EnableView
		{
			get { return Enabled; }
			set { Enabled = value; }
		}

		public void SelectFirstRowOnGrid()
		{
			GridHelper.SelectFirstRowOnGrid(grid);
		}

		public void ShowView()
		{
			Show();
		}

		public void UpdatePageOfStatusText()
		{
			var currentPage = _model.CurrentPage;
			if (_model.NumberOfPages == 0) currentPage = 0;

			var pages = string.Format(CultureInfo.CurrentCulture, Resources.PageOf, currentPage, _model.NumberOfPages);
			pageOfPagesStatusLabel.Text = pages;
		}

		public void UpdateHeaderText()
		{
			Text = _model.HeaderText;
		}

		private void backgroundWorkerDataLoaderDoWork(object sender, DoWorkEventArgs e)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(_owner.SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents);
				uow.Reassociate(dataToReassociate(null));
				_presenter.DoWork(e);
			}
		}

		private IEnumerable<IAggregateRoot>[] dataToReassociate(IPerson personToReassociate)
		{
			IEnumerable<IAggregateRoot> personsToReassociate =
				personToReassociate != null
					? new[] {personToReassociate}
					: new IAggregateRoot[] {};
			return new[]
					   {
						   new IAggregateRoot[] {_owner.SchedulerState.SchedulerStateHolder.RequestedScenario},
						   personsToReassociate,
						   _owner.MultiplicatorDefinitionSet,
						   _owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Absences,
						   _owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs,
						   _owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities,
						   _owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories
					   };
		}

		private void backgroundWorkerDataLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_presenter.WorkCompleted(e);
		}

		private void linkLabelEarlierLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_presenter.LinkLabelEarlierClicked();
		}

		private void linkLabelLaterLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_presenter.LinkLabelLaterClicked();
		}

		public bool LinkLabelLaterStatus
		{
			get { return linkLabelNext.Enabled; }
			set { linkLabelNext.Enabled = value; }
		}

		public bool LinkLabelEarlierVisibility
		{
			get { return linkLabelNext.Visible; }
			set { linkLabelNext.Visible = value; }
		}

		public void SetRestoreButtonStatus()
		{
			if (_model.PageRows.Count <= 0 ||!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment)) buttonRestore.Enabled = false;
			else buttonRestore.Enabled = true;
		}


		public bool LinkLabelEarlierStatus
		{
			get { return linkLabelPrevious.Enabled; }
			set { linkLabelPrevious.Enabled = value; }
		}

		public bool LinkLabelLaterVisibility
		{
			get { return linkLabelPrevious.Visible; }
			set { linkLabelPrevious.Visible = value; }
		}


		public void StartBackgroundWork(AuditHistoryDirection direction)
		{
			backgroundWorkerDataLoader.RunWorkerAsync(direction);
		}

		public void ShowWaitCursor()
		{
			Cursor.Current = Cursors.WaitCursor;
		}

		public void ShowDefaultCursor()
		{
			Cursor.Current = Cursors.Default;
		}

		public void ShowDataSourceException(DataSourceException dataSourceException)
		{
			using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.ViewScheduleHistory, Resources.ServerUnavailable))
			{
				view.ShowDialog(this);
			}
		}

		  private void auditHistoryViewResizeEnd(object sender, EventArgs e)
		  {
			  RefreshGrid();
		  }
	}
}
