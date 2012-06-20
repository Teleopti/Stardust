using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentDisplayRowEventArgs : EventArgs
	{
		private readonly AgentRestrictionsDisplayRow _agentDisplayRow;
			
		public AgentDisplayRowEventArgs(AgentRestrictionsDisplayRow agentRestrictionsDisplayRow)
		{
			_agentDisplayRow = agentRestrictionsDisplayRow;
		}

		public AgentRestrictionsDisplayRow AgentRestrictionsDisplayRow
		{
			get { return _agentDisplayRow; }
		}
	}

	public partial class AgentRestrictionGrid : GridControl, IAgentRestrictionsView
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsModel _model;
		private IAgentRestrictionsWarningDrawer _warningDrawer;
		private IAgentRestrictionsDrawer _loadingDrawer;
		private IAgentRestrictionsDrawer _notAvailableDrawer;
		private IAgentRestrictionsDrawer _availableDrawer;
		private RestrictionSchedulingOptions _schedulingOptions;
		private ISchedulerStateHolder _stateHolder;
		private IWorkShiftWorkTime _workShiftWorkTime;
		private int _loadedCounter;
		private IList<IPerson> _persons;
		private IPerson _selectedPerson;
		//private bool _useSchedule;

		private delegate void GridDelegate();

		public event EventHandler SelectedAgentIsReady;

		public void OnSelectedAgentIsReady(EventArgs e)
		{
			var handler = SelectedAgentIsReady;
			if (handler != null) handler(this, e);
		}

		public AgentRestrictionGrid()
		{
			InitializeComponent();
			InitializeGrid();
		}

		public AgentRestrictionGrid(IContainer container)
		{
			if(container == null) throw new ArgumentNullException("container");
			container.Add(this);
			InitializeComponent();
			InitializeGrid();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeGrid()
		{
			_warningDrawer = new AgentRestrictionsWarningDrawer();
			_loadingDrawer = new AgentRestrictionsLoadingDrawer();
			_notAvailableDrawer = new AgentRestrictionsNotAvailableDrawer();
			_availableDrawer = new AgentRestrictionsAvailableDrawer();
		
			_model = new AgentRestrictionsModel();
			_presenter = new AgentRestrictionsPresenter(this, _model, _warningDrawer, _loadingDrawer, _notAvailableDrawer, _availableDrawer);
			
			ResetVolatileData();
			GridHelper.GridStyle(this);
			InitializeHeaders();
			QueryColCount += GridQueryColCount;
			QueryRowCount += GridQueryRowCount;
			QueryCellInfo += GridQueryCellInfo;
			CellDrawn += GridCellDrawn;
			CellClick += AgentRestrictionGridCellClick;
			SelectionChanging += GridSelectionChanging;
			SelectionChanged += GridSelectionChanged;

			if (!CellModels.ContainsKey("NumericReadOnlyCellModel")) CellModels.Add("NumericReadOnlyCellModel",new NumericReadOnlyCellModel(Model) {NumberOfDecimals = 0});
			if (!CellModels.ContainsKey("TimeSpan")) CellModels.Add("TimeSpan", new TimeSpanLongHourMinutesStaticCellModel(Model));
		}

		void AgentRestrictionGridCellClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex == 1) _presenter.Sort(e.ColIndex);
			if (e.RowIndex <= 1) return;
			var displayRow = this[e.RowIndex, 0].Tag as AgentRestrictionsDisplayRow;
			if (displayRow == null) return;
			_selectedPerson = displayRow.Matrix.Person;
			if (displayRow.State != AgentRestrictionDisplayRowState.Available) return;
			
			var displayRowArgs = new AgentDisplayRowEventArgs(displayRow);
			OnSelectedAgentIsReady(displayRowArgs);
		}

		void GridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (e.Range.RangeType == GridRangeInfoType.Cells || Selections.Count > 1 || Selections.Ranges.Count > 0 && Selections.Ranges[0].Top != Selections.Ranges[0].Bottom)
			{
				var top = Selections.Ranges[0].Top;
				Selections.Clear();
				var rangelistTemp = new GridRangeInfoList {GridRangeInfo.Row(top)};
				Selections.SelectRange(rangelistTemp[0], true);							
			}
		}

		void GridSelectionChanging(object sender, GridSelectionChangingEventArgs e)
		{
			if (e.Reason == GridSelectionReason.MouseMove)
				e.Cancel = true;

			if (!e.Cancel && e.Range.Top <= 1 && e.Range.RangeType != GridRangeInfoType.Empty) 
				e.Cancel = true; 
		}

		private void InitializeHeaders()
		{
			Rows.HeaderCount = 1; // = 2 headers
			Cols.HeaderCount = 0; // = 1 header
			Rows.FrozenCount = 1;
		}

		public void MergeHeaders()
		{
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 0, 6));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 7, 0, 8));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 9, 0, 12));	
		}

		public void LoadData(ISchedulerStateHolder stateHolder, IList<IPerson> persons, RestrictionSchedulingOptions schedulingOptions, IWorkShiftWorkTime workShiftWorkTime, IPerson selectedPerson)
		{
			if (stateHolder == null) throw new ArgumentNullException("stateHolder");

			_stateHolder = stateHolder;
			_workShiftWorkTime = workShiftWorkTime;
			_schedulingOptions = schedulingOptions;
			_persons = persons;
			_selectedPerson = selectedPerson;
			_loadedCounter = 0;
			//_useSchedule = useSchedule;

			var scheduleMatrixListCreator = new ScheduleMatrixListCreator(stateHolder.SchedulingResultState);
			var agentRestrictionsDisplayRowCreator = new AgentRestrictionsDisplayRowCreator(stateHolder, scheduleMatrixListCreator);

			ThreadPool.QueueUserWorkItem(Load, agentRestrictionsDisplayRowCreator);	
		}

		public void LoadData(RestrictionSchedulingOptions schedulingOptions)
		{
			_loadedCounter = 0;
			//_useSchedule = useSchedule;
			_schedulingOptions = schedulingOptions;

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				agentRestrictionsDisplayRow.State = AgentRestrictionDisplayRowState.NotAvailable;
			}

			Invalidate();

			ThreadPool.QueueUserWorkItem(Load, null);
		}

		void Load(object workObject)
		{
			var agentRestrictionsDisplayRowCreator = workObject as AgentRestrictionsDisplayRowCreator;

			if (agentRestrictionsDisplayRowCreator != null)
			{
				_model.LoadDisplayRows(agentRestrictionsDisplayRowCreator, _persons);

				if (!IsHandleCreated) return;

				Invoke(new GridDelegate(RefreshGrid));
				Invoke(new GridDelegate(InvalidateGrid));
				Invoke(new GridDelegate(SelectRowForSelectedAgent));
			}

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				if (agentRestrictionsDisplayRow.Matrix.Person.Equals(_selectedPerson)) ThreadPool.QueueUserWorkItem(DoWork, agentRestrictionsDisplayRow);
			}

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				if (!agentRestrictionsDisplayRow.Matrix.Person.Equals(_selectedPerson)) ThreadPool.QueueUserWorkItem(DoWork, agentRestrictionsDisplayRow);
			}		
		}

		void DoWork(object workObject)
		{
			var displayRow = workObject as AgentRestrictionsDisplayRow;
			if (displayRow == null) return;
			displayRow.State = AgentRestrictionDisplayRowState.Loading;

			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
			IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor = new PossibleMinMaxWorkShiftLengthExtractor(restrictionExtractor, _workShiftWorkTime);
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
			IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator = new WorkShiftWeekMinMaxCalculator();
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator = new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator,workShiftWeekMinMaxCalculator);
			var periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
			var dataExtractor = new AgentRestrictionsDisplayDataExtractor(schedulePeriodTargetTimeCalculator, workShiftMinMaxCalculator,periodScheduledAndRestrictionDaysOff,restrictionExtractor);

			dataExtractor.ExtractTo(displayRow, _schedulingOptions); //<- TODO use schedules?
			displayRow.SetWarnings();
			displayRow.State = AgentRestrictionDisplayRowState.Available;

			if (!IsHandleCreated) return;

			if (displayRow.Matrix.Person.Equals(_selectedPerson))
			{
				var displayRowArgs = new AgentDisplayRowEventArgs(displayRow);
				OnSelectedAgentIsReady(displayRowArgs);
			}

			_loadedCounter++;
			if (_loadedCounter % 25 == 0 || _loadedCounter >= _model.DisplayRows.Count - 1) Invoke(new GridDelegate(InvalidateGrid));		
		}

		private void InvalidateGrid()
		{
			Invalidate();
			Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, 12), GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
		}

		public void RefreshGrid()
		{
			Refresh();	
		}

		private void SelectRowForSelectedAgent()
		{
			var row = -1;
			for (var i = 1; i <= RowCount; i++)
			{
				var displayRow = Model[i, 0].Tag as AgentRestrictionsDisplayRow;
				if (displayRow == null) continue;
				if (!displayRow.Matrix.Person.Equals(_selectedPerson)) continue;
				row = i;
				break;
			}

			var info = GridRangeInfo.Cells(row, 0, row, 0);

			Selections.Clear(true);
			CurrentCell.Activate(row, 0, GridSetCurrentCellOptions.SetFocus);
			Selections.ChangeSelection(info, info, true);
			CurrentCell.MoveTo(row, 0, GridSetCurrentCellOptions.ScrollInView);
		}

		void GridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryColCount;
			e.Handled = true;
		}

		void GridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _presenter.GridQueryRowCount;
			e.Handled = true;
		}

		void GridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			_presenter.GridQueryCellInfo(sender, e);
			e.Handled = true;
		}

		void GridCellDrawn(object sender, GridDrawCellEventArgs e)
		{
			_presenter.GridCellDrawn(e);
		}
	}
}
