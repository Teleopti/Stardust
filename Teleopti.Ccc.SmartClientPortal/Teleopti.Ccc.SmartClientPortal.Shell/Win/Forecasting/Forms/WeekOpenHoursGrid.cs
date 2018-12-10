using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public partial class WeekOpenHoursGrid : GridControl
    {
        private IList<DayOfWeek> _weekDays = new List<DayOfWeek>();
        private int _resolution = 15;
        private IWorkload _workload;

        public WeekOpenHoursGrid()
        {
            InitializeComponent();

            if (!CellModels.ContainsKey("TimePeriodCell"))
            {
                CellModels.Add("TimePeriodCell", new TimePeriodCellModel(Model));
            }
            SetColors();

            CellButtonClicked += WeekOpenHoursGrid_CellButtonClicked;
            ClipboardPaste += WeekOpenHoursGrid_ClipboardPaste;
            CurrentCellKeyDown += WeekOpenHoursGrid_CurrentCellKeyDown;
            KeyDown += WeekOpenHoursGrid_KeyDown;
            GotFocus += WeekOpenHoursGrid_GotFocus;
            ResizeColsBehavior = GridResizeCellsBehavior.None;
            ResizeRowsBehavior = GridResizeCellsBehavior.None;
        }

        void WeekOpenHoursGrid_GotFocus(object sender, EventArgs e)
        {
            CurrentCell.MoveTo(2, 2);
        }


        private void WeekOpenHoursGrid_KeyDown(object sender, KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(this, e);
        }

        private void WeekOpenHoursGrid_CellButtonClicked(object sender, GridCellButtonClickedEventArgs e)
        {
            GridControl grid = (GridControl)sender;
            bool isDayIsClosed = isClosed(grid, e.RowIndex, e.ColIndex);
            OpenHourDialog openHourDialog = new OpenHourDialog(getPeriodToShow(grid[e.RowIndex, e.ColIndex]), _workload, isDayIsClosed);
            openHourDialog.ShowDialog(this);
            if (openHourDialog.DialogResult == DialogResult.OK)
            {
                AddOpenHour(e.RowIndex, openHourDialog.OpenHourPeriod);
                if (openHourDialog.IsOpenHoursClosed)
                {
                    ((List<TimePeriod>)grid[e.RowIndex, 1].CellValue).Clear();
                    grid.Refresh();
                }
            }
        }

        private void WeekOpenHoursGrid_CurrentCellKeyDown(object sender, KeyEventArgs e)
        {
            
			GridControl grid = (GridControl)sender;

            if (grid.CurrentCell.ColIndex == 0)
                return;

			if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.F4) && grid.CurrentCell.RowIndex != 0)
            {
                bool isDayIsClosed = isClosed(grid, grid.CurrentCell.RowIndex, 1);
                OpenHourDialog openHourDialog = new OpenHourDialog(getPeriodToShow(grid.CurrentCell.Renderer.CurrentStyle), _workload, isDayIsClosed);
                openHourDialog.ShowDialog(this);

                if (openHourDialog.DialogResult == DialogResult.OK)
                {
                    AddOpenHour(CurrentCell.RowIndex, openHourDialog.OpenHourPeriod);
                    if (openHourDialog.IsOpenHoursClosed)
                    {
                        ((List<TimePeriod>)grid[grid.CurrentCell.RowIndex, 1].CellValue).Clear();
                    }
                    grid.Refresh();
                }
            }
        }

        private void WeekOpenHoursGrid_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            e.IgnoreCurrentCell = true;
        	var gridModel = (GridModel) sender;
			if (gridModel.CurrentCellInfo != null && gridModel.CurrentCellInfo.ColIndex == 1)
            {
                HandlePaste();
            }
            e.Handled = true;
            Refresh();
        }

        //using text in clipboard so we can paste to/from other programs like excel
        private void HandlePaste()
        {
            ClipHandler clipHandler = GridHelper.ConvertClipboardToClipHandler();

            if (clipHandler.ClipList.Count > 0)
            {
                GridRangeInfoList rangelist = GridHelper.GetGridSelectedRanges(this, true);

                foreach (GridRangeInfo range in rangelist)
                {
                    //loop all rows in selection, step with height in clip
                    for (int i = range.Top; i <= range.Bottom; i = i + clipHandler.RowSpan())
                    {
                        int row = i;

                        //loop all columns in selection, step with in clip
                        for (int j = range.Left; j <= range.Right; j = j + clipHandler.ColSpan())
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
            }
        }

        private void Paste(Clip clip, int rowIndex, int columnIndex)
        {
            if (columnIndex <= int.MinValue)
            {
                throw new ArgumentOutOfRangeException("columnIndex", "columnIndex must be larger than Int32.MinValue");
            }
            string clipValue = (string)clip.ClipObject;

            string openHourString = Convert.ToString(clipValue, CultureInfo.CurrentCulture);
            IList<TimePeriod> timePeriods = new List<TimePeriod>();
            
            if (String.IsNullOrEmpty(openHourString) ||
                openHourString.ToUpper(CultureInfo.CurrentUICulture) ==
                UserTexts.Resources.Closed.ToUpper(CultureInfo.CurrentUICulture))
            {
                var cellValue = this[rowIndex, columnIndex].CellValue as List<TimePeriod>;
                if (cellValue != null)
                {
                    if(cellValue.Count > 0)
                        cellValue.Clear();
                }
            }
            else
            {
                try
                {
                    TimePeriod openHour =
                        WinCode.Forecasting.OpenHourHelper.OpenHourFromString(openHourString,
                                                                              _resolution, _workload.Skill.MidnightBreakOffset);
                    timePeriods.Add(openHour);
                    CurrentCell.MoveTo(rowIndex, columnIndex, GridSetCurrentCellOptions.SetFocus);
                    AddOpenHour(rowIndex, openHour);
                }
                catch (ArgumentException ex)
                {
                    ViewBase.ShowErrorMessage(ex.Message, UserTexts.Resources.UnknownTimeFormat);
                }
            }
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        private void AddOpenHour(int row, TimePeriod period)
        {
            if (this[row, 1].CellValue.GetType() != typeof(List<TimePeriod>))
            {
                this[row, 1].CellValue = new List<TimePeriod>();
            }
            //Multiple openHours should not be implemented yet
            ((List<TimePeriod>)this[row, 1].CellValue).Clear();
            ((List<TimePeriod>)this[row, 1].CellValue).Add(period);

            if (period == new TimePeriod())
            {
                this[row, 1].CellTipText = ""; //Closed
            }
            else
            {
                this[row, 1].CellTipText = period.ToShortTimeString();
            }

            Refresh();
        }

        public void AddTimesToWorkload(IWorkload workload)
        {
            for (int i = 0; i <= 6; i++)
            {
                DayOfWeek day = _weekDays[i];

                if (this[i + 1, 1].CellValue.GetType() != typeof(string))
                {
                    IList<TimePeriod> openHourPeriods = (IList<TimePeriod>)this[i + 1, 1].CellValue;

                    IWorkloadDayTemplate workloadDayTemplate = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, day);
                    workloadDayTemplate.ChangeOpenHours(openHourPeriods);
                    workload.SetTemplateAt((int)day, workloadDayTemplate);
                }
            }
        }

        public void LoadDays(IWorkload workload)
        {
            _resolution = workload.Skill.DefaultResolution;
            _weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
            _workload = workload;
            

            this[0, 0].CellType = "Header";
            this[0, 0].Text = UserTexts.Resources.WeekDay;
            this[0, 0].ReadOnly = true;
            this[0, 1].CellType = "Header";
            this[0, 1].Text = UserTexts.Resources.OpenHours;
            this[0, 1].ReadOnly = true;
            int cellCount = 1;

            foreach (DayOfWeek weekDay in _weekDays)
            {
                this[cellCount, 1].CellValue = new List<TimePeriod>();

                IWorkloadDayTemplate workloadDayTemplate = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, weekDay);

                if (workloadDayTemplate.OpenHourList.Count > 0)
                {
                    CurrentCell.MoveTo(cellCount, 1, GridSetCurrentCellOptions.SetFocus);
                    foreach (TimePeriod period in workloadDayTemplate.OpenHourList)
                    {
                        AddOpenHour(CurrentCell.RowIndex, period);
                    }
                }

                this[cellCount, 1].CellType = "TimePeriodCell";

                this[cellCount, 0].CellType = "Header";
				this[cellCount, 0].Text = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(weekDay).Capitalize();
                this[cellCount, 0].ReadOnly = true;
                cellCount++;
            }
            ColWidths.ResizeToFit(GridRangeInfo.Cols(0, 0));
            ColWidths[1] = ClientRectangle.Width - ColWidths[0];
            Refresh();
        }

        private static bool isClosed(GridControl grid, int rowindex, int colIndex)
        {
	        var isClosed = ((IList<TimePeriod>)grid[rowindex, colIndex].CellValue).Count == 0;
	        return isClosed;
        }

        private static TimePeriod getPeriodToShow(GridStyleInfo gridStyleInfo)
        {
            var periods = gridStyleInfo.CellValue as ICollection<TimePeriod>;
            var period = new TimePeriod();
            if (periods == null) periods = gridStyleInfo.Tag as ICollection<TimePeriod>;
	        if (periods == null) return period;
	        if (periods.Count > 0)
	        {
		        period = periods.First();
	        }
	        return period;
        }
    }
}