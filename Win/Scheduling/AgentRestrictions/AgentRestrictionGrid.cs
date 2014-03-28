using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Secret.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;
using IContainer = System.ComponentModel.IContainer;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentDisplayRowEventArgs : EventArgs
	{
		private readonly AgentRestrictionsDisplayRow _agentDisplayRow;
		private readonly bool _moveToDate;
		private readonly bool _updateShiftEditor;

		public AgentDisplayRowEventArgs(AgentRestrictionsDisplayRow agentRestrictionsDisplayRow, bool moveToDate, bool updateShiftEditor)
		{
			_agentDisplayRow = agentRestrictionsDisplayRow;
			_moveToDate = moveToDate;
			_updateShiftEditor = updateShiftEditor;
		}

		public AgentRestrictionsDisplayRow AgentRestrictionsDisplayRow
		{
			get { return _agentDisplayRow; }
		}

		public bool MoveToDate
		{
			get { return _moveToDate; }
		}

		public bool UpdateShiftEditor
		{
			get { return _updateShiftEditor; }
		}
	}

	internal class PersonsToLoad
	{
		private readonly ICollection<IPerson> _personsToLoad;

		public PersonsToLoad()
		{
			_personsToLoad = new Collection<IPerson>();	
		}
		
		public void AddPerson(IPerson person)
		{
			_personsToLoad.Add(person);	
		}

		public bool Contains(IPerson person)
		{
			return _personsToLoad.Contains(person);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class AgentRestrictionGrid : GridControl, IAgentRestrictionsView, IHelpContext
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
		private AgentRestrictionsDetailView _detailView;
		private IAgentRestrictionsDisplayRow _currentDisplayRow;
		private IScheduleDay _selectedDay;
		private bool _moveToDate;
		private bool _clearSelection;
		private IAgentRestrictionsNoWorkShiftfFinder _agentRestrictionsNoWorkShiftfFinder;

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

			AllowSelection = GridSelectionFlags.Cell;
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
			if (!CellModels.ContainsKey("TimeSpan")) CellModels.Add("TimeSpan", new TimeSpanDurationStaticCellModel(Model));
		}

		void AgentRestrictionGridCellClick(object sender, GridCellClickEventArgs e)
		{
			if(e.ColIndex == 0 && e.RowIndex > 1)
			{
				CurrentCell.MoveTo(e.RowIndex, 1, GridSetCurrentCellOptions.ScrollInView);
			}
			if (e.RowIndex == 1)
			{
				_presenter.Sort(e.ColIndex);
				for (int r = 2; r < RowCount; r++)
				{
					var displayRow = this[r, 0].Tag as AgentRestrictionsDisplayRow;
					if (CurrentDisplayRow.Equals(displayRow))
						CurrentCell.MoveTo(r, 1, GridSetCurrentCellOptions.ScrollInView);
					
				}
			}
			if(e.RowIndex == 0)
			{
				for (int r = 2; r < RowCount; r++)
				{
					var displayRow = this[r, 0].Tag as AgentRestrictionsDisplayRow;
					if (CurrentDisplayRow.Equals(displayRow))
						CurrentCell.MoveTo(r, 1, GridSetCurrentCellOptions.ScrollInView);

				}
			}
			e.Cancel = true;
		}

		void LoadDetails(AgentRestrictionsDisplayRow displayRow)
		{
			if (displayRow == null) return;

			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
			_detailView.LoadDetails(displayRow.Matrix, restrictionExtractor, _schedulingOptions, displayRow.ContractTargetTime);

			var displayRowArgs = new AgentDisplayRowEventArgs(displayRow, false, _clearSelection);
			_clearSelection = false;
			OnSelectedAgentIsReady(displayRowArgs);
		}

		void GridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			var displayRow = this[e.Range.Top, 0].Tag as AgentRestrictionsDisplayRow;
			if (displayRow == null) return;
			
            if (displayRow.State != AgentRestrictionDisplayRowState.Available)
			{
				return;
			}

			_selectedPerson = displayRow.Matrix.Person;
			if (!CurrentDisplayRow.Equals(displayRow))
				_clearSelection = true;
			_currentDisplayRow = displayRow;

			LoadDetails(displayRow);
		}

		void GridSelectionChanging(object sender, GridSelectionChangingEventArgs e)
		{
			if (e.Range.Height != 1)
				e.Cancel = true;
		}

		private void InitializeHeaders()
		{
			Rows.HeaderCount = 1; // = 2 headers
			Cols.HeaderCount = 0; // = 1 header
			Rows.FrozenCount = 1;

			NumberedColHeaders = false;
		}

		public void MergeHeaders()
		{
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 2, 0, 6));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 7, 0, 8));
			Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 9, 0, 12));	
		}

		public void LoadData(ISchedulerStateHolder stateHolder, IList<IPerson> persons, RestrictionSchedulingOptions schedulingOptions, IWorkShiftWorkTime workShiftWorkTime, IPerson selectedPerson, AgentRestrictionsDetailView detailView, IScheduleDay scheduleDay, ILifetimeScope container)
		{
			if (stateHolder == null) throw new ArgumentNullException("stateHolder");

			_stateHolder = stateHolder;
			_workShiftWorkTime = workShiftWorkTime;
			_schedulingOptions = schedulingOptions;
			_persons = persons;
			_selectedPerson = selectedPerson;
			_loadedCounter = 0;
			_detailView = detailView;
			_selectedDay = scheduleDay;
			_moveToDate = true;

			_model.DisplayRows.Clear();

			_agentRestrictionsNoWorkShiftfFinder = container.Resolve<IAgentRestrictionsNoWorkShiftfFinder>();
			var locker = container.Resolve<IMatrixUserLockLocker>();
			var scheduleMatrixListCreator = new ScheduleMatrixListCreator(stateHolder.SchedulingResultState);
			var agentRestrictionsDisplayRowCreator = new AgentRestrictionsDisplayRowCreator(stateHolder, scheduleMatrixListCreator, locker);

			Load(agentRestrictionsDisplayRowCreator);
		}

		public void LoadData(RestrictionSchedulingOptions schedulingOptions)
		{
			if (_stateHolder == null) return;
			_loadedCounter = 0;
			_schedulingOptions = schedulingOptions;

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				agentRestrictionsDisplayRow.State = AgentRestrictionDisplayRowState.NotAvailable;
			}

			Invalidate();

			Load(null);
		}

		public void LoadData(RestrictionSchedulingOptions schedulingOptions, ICollection<IPerson> persons)
		{
			if (persons == null) return;

			_loadedCounter = 0;
			_schedulingOptions = schedulingOptions;
			var personsToLoad = new PersonsToLoad();

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				if (!persons.Contains(agentRestrictionsDisplayRow.Matrix.Person)) continue;
				agentRestrictionsDisplayRow.State = AgentRestrictionDisplayRowState.NotAvailable;
				personsToLoad.AddPerson(agentRestrictionsDisplayRow.Matrix.Person);
			}

			Invalidate();

			LoadDataPersons(personsToLoad);
		}

		void LoadDataPersons(object workObject)
		{
			var persons = workObject as PersonsToLoad;
			if (persons == null) return;

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				if(persons.Contains(agentRestrictionsDisplayRow.Matrix.Person))
					DoWork(agentRestrictionsDisplayRow);
			}
		}

		void Load(object workObject)
		{
			var agentRestrictionsDisplayRowCreator = workObject as AgentRestrictionsDisplayRowCreator;

			if (agentRestrictionsDisplayRowCreator != null)
			{
				_model.LoadDisplayRows(agentRestrictionsDisplayRowCreator, _persons);

				_currentDisplayRow = ShowRow(_model.DisplayRows);

				if (!IsHandleCreated) return;

				Invoke(new GridDelegate(RefreshGrid));
				Invoke(new GridDelegate(InvalidateGrid));
				Invoke(new GridDelegate(SelectRowForSelectedAgent));
			}

			foreach (var agentRestrictionsDisplayRow in _model.DisplayRows)
			{
				DoWork(agentRestrictionsDisplayRow);
			}
		}

		IAgentRestrictionsDisplayRow ShowRow(IList<AgentRestrictionsDisplayRow> displayRows)
		{
			if(displayRows.Count == 0) return null;

			foreach (var agentRestrictionsDisplayRow in displayRows)
			{
				if (agentRestrictionsDisplayRow.Matrix.Person.Equals(_selectedPerson) && _selectedDay != null)
				{
					if (agentRestrictionsDisplayRow.Matrix.SchedulePeriod.DateOnlyPeriod.Contains(_selectedDay.DateOnlyAsPeriod.DateOnly))
					{
						return agentRestrictionsDisplayRow;
					}
				}
			}

			foreach (var agentRestrictionsDisplayRow in displayRows)
			{
				if (agentRestrictionsDisplayRow.Matrix.Person.Equals(_selectedPerson)) return agentRestrictionsDisplayRow;
			}

			return displayRows[0];
		}

		void DoWork(object workObject)
		{
			if (IsDisposed || IsDisposing) return;
			var displayRow = workObject as AgentRestrictionsDisplayRow;
			if (displayRow == null) return;
			displayRow.State = AgentRestrictionDisplayRowState.Loading;

			IRestrictionExtractor restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
			IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor = new PossibleMinMaxWorkShiftLengthExtractor(restrictionExtractor, _workShiftWorkTime);
			ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
			IWorkShiftWeekMinMaxCalculator workShiftWeekMinMaxCalculator = new WorkShiftWeekMinMaxCalculator();
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator = new WorkShiftMinMaxCalculator(possibleMinMaxWorkShiftLengthExtractor, schedulePeriodTargetTimeCalculator,workShiftWeekMinMaxCalculator);
			var periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
			var dataExtractor = new AgentRestrictionsDisplayDataExtractor(schedulePeriodTargetTimeCalculator, workShiftMinMaxCalculator, periodScheduledAndRestrictionDaysOff, _agentRestrictionsNoWorkShiftfFinder);

			dataExtractor.ExtractTo(displayRow, _schedulingOptions); 
			displayRow.SetWarnings();
			displayRow.State = AgentRestrictionDisplayRowState.Available;

			if (!IsHandleCreated) return;

			if (IsDisposed || IsDisposing) return;
			if(displayRow.Equals(CurrentDisplayRow))
			{
				_detailView.LoadDetails(displayRow.Matrix, restrictionExtractor, _schedulingOptions, displayRow.ContractTargetTime);
				var displayRowArgs = new AgentDisplayRowEventArgs(displayRow, _moveToDate, false);
				_moveToDate = false;
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
				//if (!displayRow.Matrix.Person.Equals(_selectedPerson)) continue;
				if (!displayRow.Equals(CurrentDisplayRow)) continue;
				row = i;
				break;
			}

			//var info = GridRangeInfo.Cells(row, 0, row, 0);

			//Selections.Clear(true);
			//CurrentCell.Activate(row, 0, GridSetCurrentCellOptions.SetFocus);
			//Selections.ChangeSelection(info, info, true);
			CurrentCell.MoveTo(row, 1, GridSetCurrentCellOptions.ScrollInView);
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

		public bool HasHelp
		{
			get { return true; }
		}

		public string HelpId
		{
			get { return Name; }
		}

		public AgentRestrictionsDisplayRow CurrentDisplayRow
		{
			get { return (AgentRestrictionsDisplayRow)_currentDisplayRow; }
		}
	}
}
