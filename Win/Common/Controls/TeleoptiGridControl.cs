using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class TeleoptiGridControl : GridControl, ITeleoptiGridControl
    {
        internal static readonly Color ColorHolidayCell = ColorHelper.GridControlGridHolidayCellColor();
        internal static readonly Color ColorHolidayHeader = ColorHelper.GridControlGridHolidayHeaderColor();
        internal static readonly Color ColorEditableCell = ColorHelper.GridControlGridInteriorColor();
        internal static readonly Color ColorOutsideGridBackground = ColorHelper.GridControlGridExteriorColor();
        internal static readonly FontStyle FontClosedCell = GuiSettings.ClosedCellFontStyle();
        internal static readonly Color ColorFontClosedCell = GuiSettings.ClosedCellFontColor();
        private static readonly object Lock = new object();
        private static ForecasterSettings _currentSettings;
        private readonly IList<INumericCellModelWithDecimals> _numericCellModelWithDecimalses = new List<INumericCellModelWithDecimals>();
        private int _numberOfDecimals;

        public event EventHandler<DataToChartEventArgs> DataToChart;

        protected void TriggerDataToChart(GridRangeInfo gridRangeInfo)
        {
			  var handler = DataToChart;
            if (handler != null &&
                gridRangeInfo.Height==1)
            {
                DataToChartEventArgs eventArgs = new DataToChartEventArgs(
                    Model[gridRangeInfo.Top, gridRangeInfo.Left].CellModel,
                    Model[gridRangeInfo.Top, gridRangeInfo.Left - 1].CellValue.ToString(),
                    gridRangeInfo.Top,
                    GetRowDataForChart(gridRangeInfo));
                
                handler.Invoke(this,eventArgs);
            }
        }

        public void InitializeAllGridRowsToChart()
        {
            for (int i = ColHeaderCount; i <= RowCount; i++)
            {
                if (!IsChartRow(i)) continue;
                var range = GridRangeInfo.Cells(i, RowHeaderCount, i, ColCount);
                TriggerDataToChart(range);
            }
        }

        protected virtual bool IsChartRow(int rowIndex)
        {
            return true;
        }

        protected virtual IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            return new Dictionary<DateTime, double>();
        }

        public event EventHandler<GridRangePasteEventArgs> GridRangePasted;

        protected virtual void TriggerGridRangePasted(GridRangeInfo gridRangeInfo)
        {
        	var handler = GridRangePasted;
            if (handler !=null)
            {
                GridRangePasteEventArgs eventArgs = new GridRangePasteEventArgs();
                eventArgs.GridRange = gridRangeInfo;
                handler.Invoke(this, eventArgs);
            }
        }

        public virtual WorkingInterval WorkingInterval { get { return WorkingInterval.Day; } }
        public virtual TimeSpan ChartResolution { get { return TimeSpan.FromHours(1); } }
        public virtual DateTime FirstDateTime { get { return DateTime.MinValue; } }
        public virtual DateTime LastDateTime { get { return DateTime.MinValue; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiGridControl"/> class.
        /// First inherited class (See examples): TaskOwnerDayGridControl
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        public TeleoptiGridControl()
        {
            initializeDefaultValues();
            initializeGrid();
				GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
				GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
				MetroScrollBars = true;
				Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            ClipboardPaste += TeleoptiGridControl_ClipboardPaste;
            KeyDown += TeleoptiGridControl_KeyDown;
        }

        public static ForecasterSettings CurrentForecasterSettings()
        {
            lock (Lock)
            {
                if (_currentSettings == null)
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        _currentSettings = new PersonalSettingDataRepository(uow).FindValueByKey("Forecaster",
                                                                                                 new ForecasterSettings());
                        return _currentSettings;
                    }
                }
                return _currentSettings;
            }
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-08-28
        /// </remarks>
        private void initializeDefaultValues()
        {

            if (!StateHolderReader.IsInitialized)
            {
                //For use in editor
                _numberOfDecimals = 1;
                return;
            }

            _numberOfDecimals = CurrentForecasterSettings().NumericCellVariableDecimals;
            
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        private void initializeGrid()
        {
            CellModels.Add("NumericServiceTargetLimitedCell", initializeCallServiceTargetNumericCell());
            CellModels.Add("NumericWorkloadIntradayTaskLimitedCell", initializeCallWorkloadIntradayTaskNumericCell());
            CellModels.Add("NumericWorkloadDayTaskLimitedCell", initializeCallWorkloadDayTaskNumericCell());
            CellModels.Add("NumericWorkloadWeekTaskLimitedCell", initializeCallWorkloadWeekTaskNumericCell());
            CellModels.Add("NumericWorkloadMonthTaskLimitedCell", initializeCallWorkloadMonthTaskNumericCell());
            CellModels.Add("NumericCell", initializeCallNumericCell());
            CellModels.Add("NullableNumericCell", initializeNullableNumericCell());
            CellModels.Add("NumericTwoDecimalCell", initializeCallNumericTwoDecimalCell());
            CellModels.Add("PercentCell", initializeCallPercentCell());
            CellModels.Add("MultiSitePercentCell", initializeMultiSitePercentCell());
            CellModels.Add("PercentWithNegativeCell", initializePercentWithNegativeCell());
            CellModels.Add("PercentShrinkageCell", initializeShrinkagePercentCell());
            CellModels.Add("PercentEfficiencyCell", initializeEfficiencyPercentCell());
            CellModels.Add("ServicePercentCell", initializeServicePercentCell());
            CellModels.Add("TimeSpanTotalSecondsReadOnlyCell", initializeTotalSecondsReadOnlyCell());
            CellModels.Add("NumericReadOnlyCell", initializeCallNumericReadOnlyCell());
            CellModels.Add("PercentReadOnlyCell", initializeCallPercentReadOnlyCell());
            CellModels.Add("TimeSpanTotalSecondsCell", initializeTotalSeconds());
            CellModels.Add("PositiveTimeSpanTotalSecondsCell", initializePositiveTotalSeconds());
            CellModels.Add("IntegerCell", initializeIntegerCell());
            CellModels.Add("PositiveIntegerCell", initializePositiveIntegerCell());
            CellModels.Add("IntegerMinMaxAgentCell", initializeCallMinMaxAgentIntegercCell());
            CellModels.Add("TimeSpanReadOnlyCell",initializeTimeSpanReadOnlyCell());
            CellModels.Add("TimeSpanLongHourMinutesCellModel", InitializeTimeSpanLongHourMinutesCellModel());
            CellModels.Add("TimeSpanLongHourMinutesOnlyPositiveCellModel", initializeTimeSpanLongHourMinutesPositiveCellModel());
            CellModels.Add("TimeSpanLongHourMinuteSecondOnlyPositiveCellModel", initializeTimeSpanLongHourMinuteSecondPositiveCellModel());
            CellModels.Add("TimeSpanLongHourMinutesStaticCellModel", initializeTimeSpanLongHourMinutesStaticCellModel());
            CellModels.Add("IntegerReadOnlyCell", initializeNumericNoDecimalsReadOnlyCell());
            CellModels.Add("PercenFromPercentReadOnlyCellModel", initializeCallPercentReadOnlyPercentCell());
            
            GridHelper.GridStyle(this);

            BackColor = ColorEditableCell;
            Properties.BackgroundColor = ColorOutsideGridBackground;
        }

    	private NullableNumericCellModel initializeNullableNumericCell()
    	{
			var cellModel = new NullableNumericCellModel(Model) { NumberOfDecimals = _numberOfDecimals, MinValue = 0, MaxValue = 99999999d };
			_numericCellModelWithDecimalses.Add(cellModel);
    		return cellModel;
    	}

    	private GridCellModelBase initializeTimeSpanReadOnlyCell()
        {
            TimeSpanDurationStaticCellModel cellModel = new TimeSpanDurationStaticCellModel(Model);
            return cellModel;
        }
        private TimeSpanDurationCellModel InitializeTimeSpanLongHourMinutesCellModel()
        {
            TimeSpanDurationCellModel cellModel = new TimeSpanDurationCellModel(Model);

            return cellModel;
        }
        private GridCellModelBase initializeTimeSpanLongHourMinuteSecondPositiveCellModel()
        {
            var cellModel = new TimeSpanDurationCellModel(Model);
            cellModel.OnlyPositiveValues = true;
            cellModel.DisplaySeconds = true;

            return cellModel;
        }
        private TimeSpanDurationCellModel initializeTimeSpanLongHourMinutesPositiveCellModel()
        {
            TimeSpanDurationCellModel cellModel = new TimeSpanDurationCellModel(Model);
            cellModel.OnlyPositiveValues = true;

            return cellModel;
        }
        private TimeSpanDurationStaticCellModel initializeTimeSpanLongHourMinutesStaticCellModel()
        {
            TimeSpanDurationStaticCellModel cellModel = new TimeSpanDurationStaticCellModel(Model);
            cellModel.DisplaySeconds = true;

            return cellModel;
        }
        private NumericCellModel initializeCallNumericTwoDecimalCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = 2;
            cellModel.MinValue = 0;
            cellModel.MaxValue = 999;
            return cellModel;
        }

        //6 digits + decimals
        private NumericCellModel initializeCallWorkloadIntradayTaskNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            cellModel.MinValue = 0;
            cellModel.MaxValue = 999999;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }
        //7 digits + decimals
        private NumericCellModel initializeCallWorkloadDayTaskNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            cellModel.MinValue = 0;
            cellModel.MaxValue = 9999999;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }
        //8 digits + decimals
        private NumericCellModel initializeCallWorkloadWeekTaskNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            cellModel.MinValue = 0;
            cellModel.MaxValue = 99999999;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }
        //9 digits + decimals
        private NumericCellModel initializeCallWorkloadMonthTaskNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            cellModel.MinValue = 0;
            cellModel.MaxValue = 999999999;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }
        private NumericCellModel initializeCallServiceTargetNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            cellModel.MinValue = 0.1;
            cellModel.MaxValue = 5000;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }
        private IntegerCellModel initializeCallMinMaxAgentIntegercCell()
        {
            IntegerCellModel cellModel = new IntegerCellModel(Model);
            cellModel.MinValue = 0;
            cellModel.MaxValue = 2000;
            return cellModel;
        }
        private GridCellModelBase initializeCallPercentReadOnlyCell()
        {
            PercentReadOnlyCellModel cellModel = new PercentReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        private GridCellModelBase initializeIntegerCell()
        {
            IntegerCellModel cellModel = new IntegerCellModel(Model);
            return cellModel;
        }

        private GridCellModelBase initializePositiveIntegerCell()
        {
            IntegerCellModel cellModel = new IntegerCellModel(Model);
            cellModel.OnlyPositiveValues = true;
            return cellModel;
        }
        private GridCellModelBase initializeCallPercentReadOnlyPercentCell()
        {
            PercentReadOnlyCellModel cellModel = new PercentReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        /// <summary>
        /// Initializes the call numeric read only cell.
        /// note: added decimalhandling on this celltype as well
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-09-09
        /// </remarks>
        private GridCellModelBase initializeCallNumericReadOnlyCell()
        {
            NumericReadOnlyCellModel cellModel = new NumericReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
           _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }

        private GridCellModelBase initializeNumericNoDecimalsReadOnlyCell()
        {
            NumericReadOnlyCellModel cellModel = new NumericReadOnlyCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        /// <summary>
        /// Initializes the total seconds read only cell.
        /// note: added decimalhandling on this celltype as well
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-09-09
        /// </remarks>
        private GridCellModelBase initializeTotalSecondsReadOnlyCell()
        {
            TimeSpanTotalSecondsStaticCellModel cellModel = new TimeSpanTotalSecondsStaticCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }

        private GridCellModelBase initializeCallPercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            return cellModel;
        }

        private GridCellModelBase initializeMultiSitePercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 2;
            cellModel.MinMax = new MinMax<double>(0.00, 1);
            return cellModel;
        }

        private GridCellModelBase initializePercentWithNegativeCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 1;
            cellModel.MinMax = new MinMax<double>(-1.0d, 10.0d);
            return cellModel;
        }

        private GridCellModelBase initializeServicePercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            cellModel.MinMax = new MinMax<double>(0.01, 1);
            return cellModel;
        } 

        private GridCellModelBase initializeShrinkagePercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            cellModel.MinMax = new MinMax<double>(0.00, 0.99);
            return cellModel;
        }

        private GridCellModelBase initializeEfficiencyPercentCell()
        {
            PercentCellModel cellModel = new PercentCellModel(Model);
            cellModel.NumberOfDecimals = 0;
            cellModel.MinMax = new MinMax<double>(0.01, 1);
            return cellModel;
        }

        /// <summary>
        /// Initializes the call numeric cell.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        private NumericCellModel initializeCallNumericCell()
        {
            NumericCellModel cellModel = new NumericCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }

        /// <summary>
        /// Initializes the total seconds.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        private TimeSpanTotalSecondsCellModel initializeTotalSeconds()
        {
            TimeSpanTotalSecondsCellModel cellModel = new TimeSpanTotalSecondsCellModel(Model);
            cellModel.NumberOfDecimals = _numberOfDecimals;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }

        private TimeSpanTotalSecondsCellModel initializePositiveTotalSeconds()
        {
            TimeSpanTotalSecondsCellModel cellModel = new TimeSpanTotalSecondsCellModel(Model);
            cellModel.NumberOfDecimals = (int)_numberOfDecimals;
            cellModel.OnlyPositiveValues = true;
            _numericCellModelWithDecimalses.Add(cellModel);
            return cellModel;
        }

        private void TeleoptiGridControl_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            e.IgnoreCurrentCell = true;
            HandlePaste();
            e.Handled = true;
        }

        void TeleoptiGridControl_KeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(this, e);
        }

        protected override bool ProcessDialogKey(Keys key)
        {
            if (key == Keys.Tab)
            {
                Parent.Parent.SelectNextControl(this, true, true, true, true);
                return true;
            }
            return base.ProcessDialogKey(key);
        }

        /// <summary>
        /// Handles the paste.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual void HandlePaste()
        {
            ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

            if (clipHandler.ClipList.Count > 0)
            {		
                BeginUpdate();
                OnBeforePaste();
                GridRangeInfoList rangelist = GridHelper.GetGridSelectedRanges(this, true);

                int clipRowSpan = clipHandler.RowSpan();
                int clipColSpan = clipHandler.ColSpan();

                foreach (GridRangeInfo range in rangelist)
                {
                    //loop all rows in selection, step with height in clip
                    for (int i = range.Top; i <= range.Bottom; i = i + clipRowSpan)
                    {
                        int row = i;

                        //loop all columns in selection, step with in clip
                        for (int j = range.Left; j <= range.Right; j = j + clipColSpan)
                        {
                            int col = j;

                            if (row > Rows.HeaderCount && col > Cols.HeaderCount)
                            {

                                foreach (Clip clip in clipHandler.ClipList)
                                {
                                    //check clip fits inside selected range, rows
                                    if (GridHelper.IsPasteRangeOk(range, this, clip, i, j))
                                    {
                                        Paste(clip, row + clip.RowOffset, col + clip.ColOffset);
                                    }
                                }
                            }
                        }
                    }
                }
                EndUpdate();
                if (rangelist.Count > 0)
                {
                    int firstRow = rangelist.ActiveRange.Top;
                    int firstCol = rangelist.ActiveRange.Left;
                    int lastRow = firstRow + clipRowSpan - 1;
                    int lastCol = firstCol + clipColSpan - 1;
                    if (lastRow > RowCount)
                    {
                        lastRow = RowCount;
                    }
                    if (lastCol > ColCount)
                    {
                        lastCol = ColCount;
                    }
                    GridRangeInfo pastedRange = GridRangeInfo.Auto(firstRow, firstCol, lastRow, lastCol);
                    RefreshRange(pastedRange);
                    TriggerGridRangePasted(pastedRange);
                }
                OnAfterPaste();
                Refresh();
            }
        }

        /// <summary>
        /// Pastes the specified clip.
        /// </summary>
        /// <param name="clip">The clip.</param>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-11
        /// </remarks>
        public virtual void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }
            
            GridStyleInfo gsi = this[rowIndex, columnIndex];
            string clipValue = (string) clip.ClipObject;
            if (clipValue.Length <= gsi.MaxLength || gsi.MaxLength == 0)
                gsi.ApplyFormattedText(clipValue);
        }

        protected static TextManager GetManager(AbstractDetailView owner)
        {
            TextManager manager = new TextManager(owner.SkillType);
            return manager;
        }

        /// <summary>
        /// Adds the covered range.
        /// </summary>
        /// <param name="rangeInfo">The range info.</param>
        /// <remarks>
        /// Wrapper created for testing purposes!
        /// Created by: robink
        /// Created date: 2008-10-24
        /// </remarks>
        public void AddCoveredRange(GridRangeInfo rangeInfo)
        {
            CoveredRanges.Add(rangeInfo);
        }

        protected virtual void OnBeforePaste()
        {
        }

        protected virtual void OnAfterPaste()
        {
        }

        protected CellInfo GetCellInfo(GridStyleInfo style,int colIndex,int rowIndex)
        {
            return new CellInfo
                       {
                           ColCount = ColCount,
                           ColHeaderCount = ColHeaderCount,
                           ColIndex = colIndex,
                           Handled = false,
                           RowCount = RowCount,
                           RowHeaderCount = RowHeaderCount,
                           RowIndex = rowIndex,
                           Style = style
                       };
        }

        public int ChangeNumberOfDecimals(int changeCount)
        {
            int currentNumberOfDecimals = 0;
            foreach (INumericCellModelWithDecimals numericCellModelWithDecimals in _numericCellModelWithDecimalses)
            {
                currentNumberOfDecimals = numericCellModelWithDecimals.NumberOfDecimals;
                if (numericCellModelWithDecimals.NumberOfDecimals < 10)
                {
                    numericCellModelWithDecimals.NumberOfDecimals += changeCount;
                }
            }
            Refresh(true);
            return currentNumberOfDecimals;
        }

        protected int ColHeaderCount
        {
            get { return Rows.HeaderCount + 1; }
        }

        protected int RowHeaderCount
        {
            get { return Cols.HeaderCount + 1; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClipboardPaste -= TeleoptiGridControl_ClipboardPaste;
                KeyDown -= TeleoptiGridControl_KeyDown;
                ResetVolatileData();
            }
            base.Dispose(disposing);
        }
    }
}
