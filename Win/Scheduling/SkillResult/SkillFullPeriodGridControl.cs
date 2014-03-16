using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Rows;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SkillResult
{
	public class SkillFullPeriodGridControl : SkillResultGridControlBase, ITaskOwnerGrid, ISkillFullPeriodGridControl
    {
        private AbstractDetailView _owner;
        private const int rowHeaderWidth = 200;
        private const string settingName = "SchedulerSkillDayGridAndChart";
		private RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> _rowManager;
        private IList<IGridRow> _gridRows;
        private GridRow _currentSelectedGridRow;
        private readonly ChartSettings _chartSettings;
        private readonly ChartSettings _defaultChartSettings = new ChartSettings();
		private readonly SkillFullPeriodGridControlPresenter _presenter;

        public SkillFullPeriodGridControl()
        {
            initializeComponent();
            initializeGrid();
            setupChartDefault();
           
            using(var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _chartSettings = new PersonalSettingDataRepository(uow).FindValueByKey(settingName, _defaultChartSettings);
            }

			_presenter = new SkillFullPeriodGridControlPresenter(this);
        }

        private void initializeComponent()
        {
            QueryColWidth += gridSkillDataQueryColWidth;
            QueryCellInfo += gridSkillDataQueryCellInfo;
            Model.ClipboardCanCopy +=Model_ClipboardCanCopy;

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);
        }

		public static int PreferredGridWidth
		{
			get { return rowHeaderWidth * 2 + System.Windows.Forms.SystemInformation.VerticalScrollBarWidth; }
		}

        private void Model_ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            e.Result = true;
            e.Handled = true;
        }

        private void setupChartDefault()
        {
            _defaultChartSettings.SelectedRows.Add("ForecastedHours");
            _defaultChartSettings.SelectedRows.Add("ScheduledHours");
            _defaultChartSettings.SelectedRows.Add("RelativeDifference");
        }

        private void initializeGrid()
        {
            CellModels.Add("TimeCell", timeSpanLongHourMinutesStaticCellModel());
            CellModels.Add("TimeSpanCell", initializeCallTimeSpanCell());
            CellModels.Add("ReadOnlyPercentCell", initializeCallPercentReadOnlyCell());
        }

        private GridCellModelBase timeSpanLongHourMinutesStaticCellModel()
        {
            return new TimeSpanDurationStaticCellModel(Model);
        }

        private GridCellModelBase initializeCallTimeSpanCell()
        {
            return new TimeSpanDurationStaticCellModel(Model);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private GridCellModelBase initializeCallPercentReadOnlyCell()
        {
			var model = new PercentReadOnlyCellModel(Model) {NumberOfDecimals = 1};
        	return model;
        }

        private void gridSkillDataQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {	
			e.Size = rowHeaderWidth;
            e.Handled = true;   
        }

        private void gridSkillDataQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (_gridRows == null) return;
            if (e.ColIndex == 0 && e.RowIndex == 1) e.Style.CellValue = " ";

            if(e.RowIndex < _gridRows.Count) _gridRows[e.RowIndex].QueryCellInfo(GetCellInfo(e.Style,e.ColIndex,e.RowIndex));
            if (e.ColIndex > 0) e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;

            e.Handled = true;
        }

        private IChartSeriesSetting configureSetting(string key)
        {
            var ret = _chartSettings.DefinedSetting(key, new ChartSettingsManager().ChartSettingsDefault);
            ret.Enabled = _chartSettings.SelectedRows.Contains(key);
            return ret;
        }

        public void CreateGridRows(ISkill skill, IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder)
        {
			if (skill == null || dates == null) return;

            ((NumericReadOnlyCellModel)CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;
            DateOnly baseDate;

            //_gridRows = new List<IGridRow> {new DateHeaderGridRow(DateHeaderType.WeekDates, dates)};
			_gridRows = new List<IGridRow> { new DateHeaderGridRow(DateHeaderType.Period, dates) };
        	baseDate = dates.Count > 0 ? dates.First() : DateOnly.MinValue;

            _rowManager = new RowManagerScheduler<SkillFullPeriodGridRow, IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>>(this, new List<IntervalDefinition>(), 15, schedulerStateHolder);
            _rowManager.BaseDate = baseDate;

        	SkillFullPeriodGridRow gridRow;

			if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
			{
				gridRow = new SkillFullPeriodGridRow(_rowManager, "TimeCell", "ForecastedHours", UserTexts.Resources.ForecastedHours);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				if (!skill.IsVirtual)
				{
					gridRow = new SkillFullPeriodGridRowMinMaxIssues(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}
				else
				{
					gridRow = new SkillFullPeriodGridRowMinMaxIssuesSummary(_rowManager, "TimeCell", "ScheduledHours", UserTexts.Resources.ScheduledHours);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}

				gridRow = new SkillFullPeriodGridRow(_rowManager, "TimeSpanCell", "AbsoluteDifference", UserTexts.Resources.AbsoluteDifference);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				if (!skill.IsVirtual)
				{
					gridRow = new SkillFullPeriodGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",UserTexts.Resources.RelativeDifference, skill);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}
				else
				{
					gridRow = new SkillFullPeriodGridRowStaffingIssuesSummary(_rowManager, "ReadOnlyPercentCell", "RelativeDifference",UserTexts.Resources.RelativeDifference);
					gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
					_gridRows.Add(_rowManager.AddRow(gridRow));
				}

				
				gridRow = new SkillFullPeriodGridRow(_rowManager, "NumericReadOnlyCell", "DailySmoothness", UserTexts.Resources.StandardDeviation);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));


                gridRow = new SkillFullPeriodGridRow(_rowManager, "ReadOnlyPercentCell", "EstimatedServiceLevel", UserTexts.Resources.ESL);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}


			if (skill.SkillType.ForecastSource == ForecastSource.Email || skill.SkillType.ForecastSource == ForecastSource.Backoffice || skill.SkillType.ForecastSource == ForecastSource.Time)
			{
				gridRow = new SkillFullPeriodGridRow(_rowManager, "TimeCell", "ForecastedHoursIncoming", UserTexts.Resources.ForecastedHoursIncoming);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillFullPeriodGridRow(_rowManager, "TimeCell", "ScheduledHoursIncoming", UserTexts.Resources.ScheduledHoursIncoming);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillFullPeriodGridRow(_rowManager, "TimeSpanCell", "AbsoluteIncomingDifference", UserTexts.Resources.AbsoluteDifferenceIncoming);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));

				gridRow = new SkillFullPeriodGridRowStaffingIssues(_rowManager, "ReadOnlyPercentCell", "RelativeIncomingDifference", UserTexts.Resources.RelativeDifferenceIncoming, skill);
				gridRow.ChartSeriesSettings = configureSetting(gridRow.DisplayMember);
				_gridRows.Add(_rowManager.AddRow(gridRow));
			}

            Rows.HeaderCount = 0;
        }
		
		public void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			var skillMonthPeriods = _presenter.CreateDataSource(stateHolder, skill);
			if (skillMonthPeriods == null) return;
			_rowManager.SetDataSource(new List<IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>> { skillMonthPeriods });
		}

		public void SetupGrid(int colCount)
		{
			ColCount = colCount;
			RowCount = _gridRows.Count - 1;
			ColWidths[0] = rowHeaderWidth;
		}

        public void DrawDayGrid(ISchedulerStateHolder stateHolder,ISkill skill)
        {
           _presenter.DrawFullPeriodGrid(stateHolder, skill);	
        }

        public void RefreshGrid()
        {
            using (PerformanceOutput.ForOperation("Refreshing SkillFullPeriodGridControl"))
            {
                Refresh();
            }
        }

        public AbstractDetailView Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public void GoToDate(DateTime theDate)
        {
            RefreshGrid();
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            if(enabled)
                _chartSettings.SelectedRows.Add(key);
            else
            {
                _chartSettings.SelectedRows.Remove(key);
            }  
        }

        public DateTime GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            IList<GridRow> ret = new List<GridRow>();
            foreach (string key in _chartSettings.SelectedRows)
            {
                foreach (GridRow gridRow in _gridRows.OfType<GridRow>())
                {
                    if(gridRow.DisplayMember == key)
                        ret.Add(gridRow);
                }
            }

            return ret;
        }

        public IDictionary<int, GridRow> EnabledChartGridRows
        {
            get
            {

                if (_gridRows == null) 
                    return new Dictionary<int, GridRow>();

                IDictionary<int, GridRow> settings = (from r in _gridRows.OfType<GridRow>()
                                                      where r.ChartSeriesSettings != null &&
                                                            r.ChartSeriesSettings.Enabled
                                                      select r).ToDictionary(k => _gridRows.IndexOf(k), v => v);

                return settings;
            }
        }

        public void SaveSetting()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                new PersonalSettingDataRepository(uow).PersistSettingValue(_chartSettings);
                uow.PersistAll();
            }
        }

        public ReadOnlyCollection<GridRow> AllGridRows
        {
            get
            {
                if (_gridRows == null) return new ReadOnlyCollection<GridRow>(new List<GridRow>());
                return new ReadOnlyCollection<GridRow>(new List<GridRow>(_gridRows.OfType<GridRow>()));
            }
        }

        public int MainHeaderRow
        {
            get { return 0; }
        }

        public bool HasColumns
        {
			get { return _presenter.FullPeriods.Count > 0; }
        }

        public GridRow CurrentSelectedGridRow
        {
            get { return _currentSelectedGridRow; }
        }

        protected override void OnSelectionChanged(GridSelectionChangedEventArgs e)
        {
			if(e == null) throw new ArgumentNullException("e");
            if (e.Range.Top > 0)
            {
                GridRow gridRow = _gridRows[e.Range.Top] as GridRow;

                if (gridRow != null)
                {
                    _currentSelectedGridRow = gridRow;
                }
            }
            base.OnSelectionChanged(e);
        }
    }
}
