using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    public partial class SeasonVolumeGridControl: TeleoptiGridControl
    {
        private VolumeYear _volumeYear;
        private WorkflowSeasonView _owner;
        private ISkillType _skillType;
        private IDictionary<int, GridRowTypes> _gridRows;
        private IList<string> _rowHeaders;
        private IList<double> _modifiedItems;
        
        public SeasonVolumeGridControl(VolumeYear volumeYear, WorkflowSeasonView owner)
        {
            _skillType = owner.Owner.Presenter.Model.Workload.Skill.SkillType;//todo: argh fix ((ValidatedVolumeDay)volumeYear.TaskOwnerDays[0]).Workload.Skill.SkillType;
            _volumeYear = volumeYear;
            _owner = owner;
            createGridRows();
            createRowHeaders();
            InitializeComponent();
            initializeGrid();
            CreateContextMenu();
	        TeleoptiStyling = true;
        }

        public void CreateContextMenu()
        {
            var gridItemModify = new MenuItem(Resources.ModifySelection, ModifySelectionOnClick);
            var menu = new ContextMenu();
            menu.MenuItems.Add(gridItemModify);
            gridItemModify.Enabled = true;
            ContextMenu = menu;
        }

        private void ModifySelectionOnClick(object sender, EventArgs e)
        {
            var modifySelectedList = _modifiedItems;
            var numbers = new ModifyCalculator(modifySelectedList);
            var modifySelection = new ModifySelectionView(numbers);
            if (modifySelection.ShowDialog(this) != DialogResult.OK) return;
            var receivedValues = modifySelection.ModifiedList;
            GridHelper.ModifySelectionInput(this, receivedValues);
        }

        protected override void OnShowContextMenu(Syncfusion.Windows.Forms.ShowContextMenuEventArgs e)
        {
            bool enableMenu;
            _modifiedItems = new List<double>();
            _modifiedItems.Clear();
            GridHelper.ModifySelectionEnabled(this, out _modifiedItems, out enableMenu);
            ContextMenu.MenuItems[0].Enabled = enableMenu;
            base.OnShowContextMenu(e);
        }

        public void SetupColumnWidths()
        {
            this.ResizeToFit();
            Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();
        }

        internal enum GridRowTypes
        {     
            HeaderSeasonTypeName = 0,
            TaskIndex,
            AverageTasks,
            TalkTimeIndex,
            AverageTalkTime,
            AfterTalkTimeIndex,
            AverageAfterWorkTime                   
        }

        #region InitializeGrid

        private void initializeGrid()
        {
            Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeColumnsInRow;
            RowCount = _rowHeaders.Count - 1;
            ColCount = _volumeYear.PeriodTypeCollection.Count;
            BaseStylesMap["Header"].StyleInfo.HorizontalAlignment = GridHorizontalAlignment.Center;

            DefaultColWidth = 50;
        }

        protected override void OnQueryRowCount(GridRowColCountEventArgs e)
        {
            base.OnQueryRowCount(e);

            e.Count = _rowHeaders.Count - 1;
            e.Handled = true;
        }

        protected override void OnQueryColCount(GridRowColCountEventArgs e)
        {
            base.OnQueryColCount(e);

            e.Count = _volumeYear.PeriodTypeCollection.Count;
            e.Handled = true;
        }

        protected override void OnResizingColumns(GridResizingColumnsEventArgs e)
        {
            base.OnResizingColumns(e);
            System.Diagnostics.Trace.WriteLine(e.Reason);
            if (e.Reason == GridResizeCellsReason.DoubleClick)
            {
                ColWidths.ResizeToFit(Selections.Ranges[0], GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
                e.Cancel = true;
            }
        }

        private void createGridRows()
        {
            _gridRows = new Dictionary<int, GridRowTypes>();

            _gridRows.Add(0, GridRowTypes.HeaderSeasonTypeName);
            _gridRows.Add(1, GridRowTypes.TaskIndex);
            _gridRows.Add(2, GridRowTypes.AverageTasks);
            _gridRows.Add(3, GridRowTypes.TalkTimeIndex);
            _gridRows.Add(4, GridRowTypes.AverageTalkTime);
            _gridRows.Add(5, GridRowTypes.AfterTalkTimeIndex);
            _gridRows.Add(6, GridRowTypes.AverageAfterWorkTime);
        }

        private void createRowHeaders()
        {
            TextManager manager = new TextManager(SkillType);
            _rowHeaders = new List<string>(_gridRows.Count);
            foreach (KeyValuePair<int, GridRowTypes> item in _gridRows)
            {
                if (SkillType.ForecastSource != ForecastSource.InboundTelephony)
                {
                    switch (item.Value)
                    {
                        case GridRowTypes.HeaderSeasonTypeName:
                            _rowHeaders.Add(" ");
                            break;
                        case GridRowTypes.TaskIndex:
                            _rowHeaders.Add(manager.WordDictionary["TaskIndex"]);
                            break;
                        case GridRowTypes.AverageTasks:
                            _rowHeaders.Add(manager.WordDictionary["Tasks"]);
                            break;
                        case GridRowTypes.TalkTimeIndex:
                            _rowHeaders.Add(manager.WordDictionary["TalkTimeIndex"]);
                            break;
                        case GridRowTypes.AverageTalkTime:
                            _rowHeaders.Add(manager.WordDictionary["AverageTaskTime"]);
                            break;
                        case GridRowTypes.AfterTalkTimeIndex:
                            _rowHeaders.Add(manager.WordDictionary["AfterTalkTimeIndex"]);
                            break;
                        case GridRowTypes.AverageAfterWorkTime:
                            _rowHeaders.Add(manager.WordDictionary["AverageAfterTaskTime"]);
                            break;
                    }
                }
                else
                {
                    switch (item.Value)
                    {
                        case GridRowTypes.HeaderSeasonTypeName:
                            _rowHeaders.Add(" ");
                            break;
                        case GridRowTypes.TaskIndex:
                            _rowHeaders.Add(UserTexts.Resources.IndexCalls);
                            break;
                        case GridRowTypes.AverageTasks:
                            _rowHeaders.Add(UserTexts.Resources.Calls);
                            break;
                        case GridRowTypes.TalkTimeIndex:
                            _rowHeaders.Add(UserTexts.Resources.IndexTalkTime);
                            break;
                        case GridRowTypes.AverageTalkTime:
                            _rowHeaders.Add(UserTexts.Resources.TalkTime);
                            break;
                        case GridRowTypes.AfterTalkTimeIndex:
                            _rowHeaders.Add(UserTexts.Resources.IndexACW);
                            break;
                        case GridRowTypes.AverageAfterWorkTime:
                            _rowHeaders.Add(UserTexts.Resources.ACW);
                            break;
                    }
                }
            }
        }

        #endregion

        #region DrawValuesInGrid

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            base.OnQueryCellInfo(e);

            if (e.ColIndex < 0 || e.RowIndex < 0) return;

            if (e.ColIndex == 0)
            {
                if (e.RowIndex < _rowHeaders.Count)
                {
                    e.Style.CellValue = _rowHeaders[e.RowIndex];
                }
            }
            else
            {
                int key = e.ColIndex; // ehh... key is the number of the month
                if (_volumeYear.PeriodTypeCollection.Count > 0)
                {
                    key = RecalculateKey(key);

                    IPeriodType periodType = _volumeYear.PeriodTypeCollection[key];
                    e.Style.Tag = periodType;

                    switch (_gridRows[e.RowIndex])
                    {
                        case GridRowTypes.HeaderSeasonTypeName:
                            e.Style.BaseStyle = "Header";
                            string headerName = GetSeasonTypeHeader(key);
                            e.Style.CellValue = headerName;
                            break;
                        case GridRowTypes.TaskIndex:
                            e.Style.CellType = "NumericTwoDecimalCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.TaskIndex;
                            break;
                        case GridRowTypes.AverageTasks:
                            e.Style.CellType = "NumericCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.AverageTasks;
                            break;
                        case GridRowTypes.TalkTimeIndex:
                            e.Style.CellType = "NumericTwoDecimalCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.TalkTimeIndex;
                            break;
                        case GridRowTypes.AverageTalkTime:
							if (SkillType.ForecastSource != ForecastSource.InboundTelephony && SkillType.ForecastSource != ForecastSource.Chat)
                                    e.Style.CellType = "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel";
                            else
                            e.Style.CellType = "PositiveTimeSpanTotalSecondsCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.AverageTalkTime;
                            break;
                        case GridRowTypes.AfterTalkTimeIndex:
                            e.Style.CellType = "NumericTwoDecimalCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.AfterTalkTimeIndex;
                            break;
                        case GridRowTypes.AverageAfterWorkTime:
							if (SkillType.ForecastSource != ForecastSource.InboundTelephony && SkillType.ForecastSource != ForecastSource.Chat)
                                e.Style.CellType = "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel";
                            else
                            e.Style.CellType = "PositiveTimeSpanTotalSecondsCell";
                            e.Style.MaxLength = 9;
                            formatCell(e.Style, periodType);
                            e.Style.CellValue = periodType.AverageAfterWorkTime;
                            break;
                    }
                }
            }
            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void formatCell(GridStyleInfo gridStyleInfo, IPeriodType periodType)
        {
            gridStyleInfo.Tag = periodType;

            gridStyleInfo.Enabled = true;
            gridStyleInfo.BackColor = ColorEditableCell;
        }

        #endregion

        //SOON finished in domain
        #region SetValuesInGrid

        protected override void OnSaveCellInfo(GridSaveCellInfoEventArgs e)
        {
            base.OnSaveCellInfo(e);

            IPeriodType periodType = (IPeriodType)e.Style.Tag;

            InsertCellValue(e.RowIndex, periodType, e.Style.CellValue.ToString());

            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "columnIndex"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "fromPaste")]
        private void InsertCellValue(int rowIndex, IPeriodType periodType, string content)
        {
            double value;
            TimeSpan timeValue;

            //Don't know might be a better way of doing this, but we need to report changes to the
            //owner that we have made changes in the grid.
            _owner.ReportChanges(true);

            switch (_gridRows[rowIndex])
            {
                case GridRowTypes.TaskIndex:
                    if (double.TryParse(content, out value))
                        periodType.TaskIndex = value;
                    break;
                case GridRowTypes.AverageTasks:
                    if (double.TryParse(content, out value))
                        periodType.AverageTasks = value;
                    break;
                case GridRowTypes.TalkTimeIndex:
                    if (double.TryParse(content, out value))
                        periodType.TalkTimeIndex = value;
                    break;
                case GridRowTypes.AverageTalkTime:
                    if(ColorHelper.TryParseSecondsTimeSpan(content, out timeValue) &&
                        timeValue != TimeSpan.Zero)
                    {
                        periodType.AverageTalkTime = timeValue;
                    }
                    break;
                case GridRowTypes.AfterTalkTimeIndex:
                    if (double.TryParse(content, out value))
                        periodType.AfterTalkTimeIndex = value;
                    break;
                case GridRowTypes.AverageAfterWorkTime:
                    if (ColorHelper.TryParseSecondsTimeSpan(content, out timeValue) &&
                        timeValue != TimeSpan.Zero)
                    {
                        periodType.AverageAfterWorkTime = timeValue;
                    }  
                    break;
            }
            Refresh();
        }

        #endregion

        #region Header types and Recalculate key

        private string GetSeasonTypeHeader(int key)
        {
            //Ok, I know, I am using the IS operator!
            //Dont see another way right now than checking the type
            string returnValue = string.Empty;

            if(_volumeYear is DayOfWeeks)
            {
                returnValue = StringHelper.Capitalize(
                    CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName((DayOfWeek) key-1));
            }else if(_volumeYear is WeekOfMonth)
            {
                returnValue = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", UserTexts.Resources.Week, key);
            }else if(_volumeYear is MonthOfYear)
            {
                returnValue = StringHelper.Capitalize(
                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(key));
            }
        
            return returnValue;
        }

        //Special Recalculate key for making the first day in week work with the culture
        private int RecalculateKey(int key)
        {
            if (_volumeYear is DayOfWeeks)
            {
                int dayIndex = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                key = key + dayIndex;
                if (key > 7) key -= 7;
            }
            return key;
        }

        #endregion

        #region CopyPaste
        

        public override void HandlePaste()
        {
            ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

            if (clipHandler.ClipList.Count > 0)
            {
                base.HandlePaste();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "periodType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "clipValue")]
        public override void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }
            int key = columnIndex;
            key = RecalculateKey(columnIndex);
            IPeriodType periodType = _volumeYear.PeriodTypeCollection[key];
            string clipValue = (string)clip.ClipObject;

            InsertCellValue(rowIndex, periodType, clipValue);
        }
        #endregion

        public override WorkingInterval WorkingInterval
        {
            get { return WorkingInterval.Custom; }
        }

        public override TimeSpan ChartResolution
        {
            get { return TimeSpan.FromDays(1); }
        }

        protected override IDictionary<DateTime, double> GetRowDataForChart(GridRangeInfo gridRangeInfo)
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override DateTime FirstDateTime
        {
            get { throw new NotImplementedException(); }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override DateTime LastDateTime
        {
            get { throw new NotImplementedException(); }
        }

        public ISkillType SkillType
        {
            get { return _skillType; }
        }
    }
}