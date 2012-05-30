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
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	class AgentRestrictionsLoadObject
	{
		public AgentRestrictionsDisplayRowCreator RowCreator { get; set; }
		//public IPerson Person { get; set; }
		public IList<IPerson> Persons { get; set; } 
	}

	public partial class AgentRestrictionGrid : GridControl, IAgentRestrictionsView
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsModel _model;
		private IAgentRestrictionsWarningDrawer _warningDrawer;
		private IAgentRestrictionsDrawer _loadingDrawer;
		private IAgentRestrictionsDrawer _notAvailableDrawer;
		private IAgentRestrictionsDrawer _availableDrawer;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulerStateHolder _stateHolder;
		private IRuleSetProjectionService _projectionService;
		private int _loadedCounter;
		private IList<IPerson> _persons; 

		private delegate void GridDelegate();

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
			SelectionChanging += GridSelectionChanging;
			SelectionChanged += GridSelectionChanged;

			if (!CellModels.ContainsKey("NumericReadOnlyCellModel")) CellModels.Add("NumericReadOnlyCellModel",new NumericReadOnlyCellModel(Model) {NumberOfDecimals = 0});
			if (!CellModels.ContainsKey("TimeSpan")) CellModels.Add("TimeSpan", new TimeSpanLongHourMinutesStaticCellModel(Model));
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

		public void LoadData(ISchedulerStateHolder stateHolder, IList<IPerson> persons, ISchedulingOptions schedulingOptions, IRuleSetProjectionService ruleSetProjectionService, IList<IPerson> selectedPersons )
		{
			if (stateHolder == null) throw new ArgumentNullException("stateHolder");

			_stateHolder = stateHolder;
			_projectionService = ruleSetProjectionService;
			_schedulingOptions = schedulingOptions;
			_persons = persons;
			_loadedCounter = 0;

			var scheduleMatrixListCreator = new ScheduleMatrixListCreator(stateHolder.SchedulingResultState);
			var agentRestrictionsDisplayRowCreator = new AgentRestrictionsDisplayRowCreator(stateHolder, scheduleMatrixListCreator);

			var loadObject = new AgentRestrictionsLoadObject { Persons = selectedPersons, RowCreator = agentRestrictionsDisplayRowCreator };
			ThreadPool.QueueUserWorkItem(LoadFirst, loadObject);	
		}

		void LoadFirst(object workObject)
		{
			var loadObject = workObject as AgentRestrictionsLoadObject;
			if (loadObject == null) return;
			var rowCreator = loadObject.RowCreator;

			_model.LoadDisplayRows(rowCreator, loadObject.Persons);
			foreach (var row in _model.DisplayRows)
			{
				DoWork(row);
				row.Loaded = true;
			}
			Invoke(new GridDelegate(RefreshGrid));
			Invoke(new GridDelegate(InvalidateGrid));

			foreach (var person in _persons)
			{
				if(!loadObject.Persons.Contains(person))
				{
					var loadingObject = new AgentRestrictionsLoadObject() {Persons = new List<IPerson>{person}, RowCreator = rowCreator};
					ThreadPool.QueueUserWorkItem(LoadSecond, loadingObject);
				}
			}	
		}

		void LoadSecond(object workObject)
		{
			var loadObject = workObject as AgentRestrictionsLoadObject;
			if (loadObject == null) return;
			var rowCreator = loadObject.RowCreator;
			var rows = _model.LoadDisplayRows(rowCreator, loadObject.Persons);
			foreach (var displayRow in rows)
			{
				if (!displayRow.Loaded) DoWork(displayRow);
			}

			_loadedCounter++;
			if (_loadedCounter % 25 == 0 || _loadedCounter >= _persons.Count - 1) Invoke(new GridDelegate(RefreshGrid));
		}

		void DoWork(object workObject)
		{
			var displayRow = workObject as AgentRestrictionsDisplayRow;
			if (displayRow == null) return;
			displayRow.State = AgentRestrictionDisplayRowState.Loading;

			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
			IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor = new PossibleMinMaxWorkShiftLengthExtractor(restrictionExtractor, _projectionService);
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator = new SchedulePeriodTargetTimeTimeCalculator();
			IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator = new WorkShiftWeekMinMaxCalculator();
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator = new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator,workShiftWeekMinMaxCalculator);
			var periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
			var dataExtractor = new AgentRestrictionsDisplayDataExtractor(workShiftMinMaxCalculator,periodScheduledAndRestrictionDaysOff,restrictionExtractor);

			dataExtractor.ExtractTo(displayRow, _schedulingOptions, true);
			CheckForWarningsTest(displayRow);
			displayRow.State = AgentRestrictionDisplayRowState.Available;
			//Invoke(new GridDelegate(InvalidateGrid));		
		}

		private void InvalidateGrid()
		{
			Invalidate();
			Model.ColWidths.ResizeToFit(GridRangeInfo.Col(0), GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
		}

		private void RefreshGrid()
		{
			Refresh();
		}

		private void CheckForWarningsTest(AgentRestrictionsDisplayRow displayRow)
		{
			
			if (!displayRow.ContractCurrentTime.Equals(displayRow.ContractTargetTime))
			{	
				displayRow.SetWarning(AgentRestrictionDisplayRowColumn.ContractTime, UserTexts.Resources.ContractTimeDoesNotMeetTheTargetTime);
			}

			if(!displayRow.CurrentDaysOff.Equals(displayRow.TargetDaysOff))
			{	
				displayRow.SetWarning(AgentRestrictionDisplayRowColumn.DaysOffSchedule, UserTexts.Resources.WrongNumberOfDaysOff);
			}
			
			if(((IAgentDisplayData)displayRow).MinimumPossibleTime > displayRow.MinMaxTime.EndTime)
			{
				displayRow.SetWarning(AgentRestrictionDisplayRowColumn.Min, UserTexts.Resources.LowestPossibleWorkTimeIsTooHigh);
			}

			if (((IAgentDisplayData)displayRow).MaximumPossibleTime < displayRow.MinMaxTime.StartTime)
			{
				displayRow.SetWarning(AgentRestrictionDisplayRowColumn.Max, UserTexts.Resources.HighestPossibleWorkTimeIsTooLow);
			}
			
			//agentInfoHelper.NumberOfWarnings = numberOfWarnings;	
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
