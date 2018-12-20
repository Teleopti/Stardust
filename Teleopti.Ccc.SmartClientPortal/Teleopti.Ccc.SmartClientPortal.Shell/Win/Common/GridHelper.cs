using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Common;

using Clipboard = System.Windows.Clipboard;
using Rectangle = System.Drawing.Rectangle;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Helper for syncfusion grid
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class GridHelper
    {
        public static LinearGradientBrush GetGradientBrush(Rectangle destinationRectangle, Color color)
        {
            //create a brush rectangle slightly bigger than dest rect
            Rectangle rect = new Rectangle(destinationRectangle.X, destinationRectangle.Y - 1, destinationRectangle.Width, destinationRectangle.Height + 2);
            //return brush
            return new LinearGradientBrush(rect, Color.WhiteSmoke, color, 90, false);
        }

        public static bool IsPasteRangeOk(GridRangeInfo range, GridControl grid, Clip clip, int row, int col)
        {
            //check clip fits inside selected range, rows
            if (((clip.RowOffset + row <= range.Bottom) && (clip.RowOffset + row >= range.Top)) || (range.Top == range.Bottom && range.Left == range.Right))
            {
                //check clip fits inside selected range, cols
                if (((clip.ColOffset + col <= range.Right) && (clip.ColOffset + col >= range.Left)) || (range.Top == range.Bottom && range.Left == range.Right))
                {
                    //check clip fits inside grid
                    if ((grid.RowCount >= clip.RowOffset + row) && (grid.ColCount >= clip.ColOffset + col))
                    {
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        public static bool IsRangesSameSize(GridRangeInfo gridRangeInfo1, GridRangeInfo gridRangeInfo2)
        {
            if (gridRangeInfo1 == null)
                throw new ArgumentNullException("gridRangeInfo1");

            if (gridRangeInfo2 == null)
                throw new ArgumentNullException("gridRangeInfo2");

            return ((gridRangeInfo1.Right - gridRangeInfo1.Left) == (gridRangeInfo2.Right - gridRangeInfo2.Left)) &&
                    ((gridRangeInfo1.Bottom - gridRangeInfo1.Top) == (gridRangeInfo2.Bottom - gridRangeInfo2.Top));
        }


        public static GridRangeInfoList GetGridSelectedRanges(GridControl grid, bool excludeHeaders)
        {
            GridRangeInfoList rangelist;
            GridRangeInfoList rangelistTemp = new GridRangeInfoList();

            if (grid.Selections.GetSelectedRanges(out rangelist, true))
            {
                foreach (GridRangeInfo range in rangelist)
                {
                    //if we have rows selected we add cells in row to a temp list
                    if (range.IsRows)
                    {
                        rangelistTemp.Add(GridRangeInfo.Cells(range.Top,
                                                              range.Left +
                                                              (excludeHeaders ? grid.Cols.HeaderCount + 1 : 0),
                                                              range.Bottom, grid.ColCount));
                    }

                    //if we have col  selected we add cells in col to a temp list
                    if (range.IsCols)
                    {
                        rangelistTemp.Add(
                            GridRangeInfo.Cells(range.Top + (excludeHeaders ? grid.Rows.HeaderCount + 1 : 0), range.Left,
                                                grid.RowCount, range.Right));
                    }

                    //if we have table selected we add cells in table to a temp list
                    if (range.IsTable)
                    {
                        rangelistTemp.Add(
                            GridRangeInfo.Cells(range.Top + (excludeHeaders ? grid.Cols.HeaderCount + 1 : 0),
                            range.Left + (excludeHeaders ? grid.Rows.HeaderCount + 1 : 0), grid.RowCount, grid.ColCount));
                    }
                }

                //add cells from selected rows and cols to range list
                foreach (GridRangeInfo range in rangelistTemp)
                {
                    rangelist.Add(range);
                }
            }

            return rangelist;
        }

        public static void ModifySelectionInput(GridControl grid, IList<double> list)
        {
            GridRangeInfoList rangelist;
            var culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;
            if (grid.Selections.GetSelectedRanges(out rangelist, true))
            {
                int top;
                int width;
                int left = GetSelection(grid, rangelist, out top, out width);

                for (var i = 0; i <= width; i++)
                {
                    var currentCell = grid[top, left + i];
                    var cellStyleInfo = grid.Model[top, left + i];
                    switch (currentCell.CellType)
                    {
                        case "NumericServiceTargetLimitedCell":
                        case "NumericCell":
                        case "NumericWorkloadDayTaskLimitedCell":
                        case "NumericWorkloadWeekTaskLimitedCell":
                        case "NumericWorkloadMonthTaskLimitedCell":
                        case "NumericWorkloadIntradayTaskLimitedCell":
                        case "NumericWithTwoDecimalsCell":
                        case "NumericTwoDecimalCell":
                            currentCell.CellModel.ApplyFormattedText(cellStyleInfo, list[i].ToString(culture), -1);
                            break;
                        case "PositiveTimeSpanTotalSecondsCell":
                            if (list[i] >= 0)
                                currentCell.CellModel.ApplyFormattedText(cellStyleInfo, CheckSecondsRange(list[i]).ToString(culture), -1);
                            break;
                        case "TimeSpanTotalSecondsCell":
                            currentCell.CellModel.ApplyFormattedText(cellStyleInfo, CheckSecondsRange(list[i]).ToString(culture), -1);
                            break;
                        case "PercentWithNegativeCell":
                        case "PercentWithTwoDecimalsCell":
                        case "PercentCell":
                        case "PercentShrinkageCell":
                            PercentageValue(grid, i, top, left, list);
                            break;
                        case "PercentEfficiencyCell":
                            PercentageValue(grid, i, top, left, list);
                            break;
                        case "MultiSitePercentCell":
                        case "ServicePercentCell":
							var t = Percent.TryParse(list[i].ToString(CultureInfo.CurrentCulture), out _);
                            if (t && (list[i] <= 100 && list[i] > 0))
                                currentCell.CellModel.ApplyFormattedText(cellStyleInfo, list[i].ToString(CultureInfo.CurrentCulture), -1);
                            break;
                        case "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel":
                            if (list[i] >= 0)
                                currentCell.CellModel.ApplyFormattedText(cellStyleInfo, TimeSpan.FromSeconds(CheckSecondsRange(list[i])).ToString(), -1);
                            break; 
                        case "IntegerMinMaxAgentCell":
                            currentCell.CellModel.ApplyFormattedText(cellStyleInfo, list[i].ToString(CultureInfo.CurrentCulture), -1);
                            break;
                    }
                }
            }
        }

       
        private static double CheckSecondsRange(double d)
        {
            double maxValue = TimeSpan.MaxValue.TotalSeconds;
            double minValue = TimeSpan.MinValue.TotalSeconds;
            if (d < maxValue && d > minValue) return d;
            return 0;
        }


        private static void PercentageValue(GridControl grid, int i, int top, int left, IList<double> list)
        {
            var currentCell = grid[top, left + i];
            var cellStyleInfo = grid.Model[top, left + i];
			var t = Percent.TryParse(list[i].ToString(CultureInfo.CurrentCulture), out _);
            if (t)
                currentCell.CellModel.ApplyFormattedText(cellStyleInfo, list[i].ToString(CultureInfo.CurrentCulture), -1);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public static void ModifySelectionEnabled(GridControl grid, out IList<double> list, out bool enableStatus)
        {
            enableStatus = false;
            list = new List<double>();
            list.Clear();

            GridRangeInfoList rangelist;

            if (grid.Selections.GetSelectedRanges(out rangelist, true))
            {
                enableStatus = true;
                int top;
                int width;
                int left = GetSelection(grid, rangelist, out top, out width);

                for (var i = 0; i <= width; i++)
                {
                    switch (grid[top, left + i].CellType)
                    {
                        case "NumericServiceTargetLimitedCell":
                        case "NumericCell":
                        case "NumericWorkloadDayTaskLimitedCell":
                        case "NumericWorkloadWeekTaskLimitedCell":
                        case "NumericWorkloadMonthTaskLimitedCell":
                        case "NumericWorkloadIntradayTaskLimitedCell":
                        case "NumericWithTwoDecimalsCell":
                        case "NumericTwoDecimalCell":
                            list.Add((double)grid[top, left + i].CellValue);
                            break;
                        case "PositiveTimeSpanTotalSecondsCell":
                        case "TimeSpanTotalSecondsCell":
                        case "TimeSpanLongHourMinuteSecondOnlyPositiveCellModel":
                            {
                                var t = (TimeSpan)grid[top, left + i].CellValue;
                                list.Add(t.TotalSeconds);
                            }
                            break;
                        case "PercentWithNegativeCell":
                        case "PercentWithTwoDecimalsCell":
                        case "PercentCell":
                        case "PercentShrinkageCell":
                        case "PercentEfficiencyCell":
                        case "MultiSitePercentCell":
                        case "ServicePercentCell":
                            {
                                var t = (Percent)grid[top, left + i].CellValue;
                                list.Add(t.Value * 100);
                            }
                            break;
                        case "IntegerMinMaxAgentCell":
                            list.Add(Convert.ToDouble((int)grid[top, left + i].CellValue));
                            break;
                        default:
                            enableStatus = false;
                            break;
                    }
                }
            }
        }

        public static void SaveAsTemplateEnabled(GridControl grid, object gridRow, out bool enableStatus)
        {
            if (grid == null) throw new ArgumentNullException("grid");
            enableStatus = false;

            GridRangeInfoList rangelist;
            if (grid.Selections.GetSelectedRanges(out rangelist, true))
            {
                if (rangelist.Count != 1) return;
                if (rangelist.ActiveRange.Width == 1 && rangelist[0].Left > grid.Rows.HeaderCount && gridRow != null)
                {
                    enableStatus = true;
                }
                else return;
            }

        }

        private static int GetSelection(GridControl grid, GridRangeInfoList rangelist, out int top, out int width)
        {
            var left = rangelist[0].Left;
            var right = rangelist[0].Right;
            top = rangelist[0].Top;

            left = Math.Max(left, 1);
            right = (right == 0) ? grid.ColCount : right;

            if (grid[top, left].CellType == "Header")
                left += 1;

            width = right - left;
            return left;
        }



        public static void GridCopySelection(GridControl grid, ClipHandler clipHandler, bool excludeHeaders)
        {
            clipHandler.Clear();

            // Get the selected ranges
            GridRangeInfoList rangelist = GetGridSelectedRanges(grid, excludeHeaders);

            // Get individual range in the rangeList
            foreach (GridRangeInfo range in rangelist)
            {
                for (int row = range.Top; row <= range.Bottom; row++)
                {
                    for (int col = range.Left; col <= range.Right; col++)
                    {
                        object cellValue = grid.Model[row, col].CellValue;
                        if (excludeHeaders && (row <= grid.Rows.HeaderCount || col <= grid.Cols.HeaderCount))
                        {
                            continue;
                        }

                        clipHandler.AddClip(row, col, cellValue);
                    }
                }
            }
        }

        public static void GridCopySelection<T>(GridControl grid, ClipHandler<T> clipHandler, bool excludeHeaders)
        {
            clipHandler.Clear();

            // Get the selected ranges
            GridRangeInfoList rangelist = GetGridSelectedRanges(grid, excludeHeaders);

            // Get individual range in the rangeList
            foreach (GridRangeInfo range in rangelist)
            {
                for (int row = range.Top; row <= range.Bottom; row++)
                {
                    for (int col = range.Left; col <= range.Right; col++)
                    {
                        object cellValue = grid.Model[row, col].CellValue;
                        if (!(cellValue is T)) continue;
                        if (excludeHeaders && (row <= grid.Rows.HeaderCount || col <= grid.Cols.HeaderCount))
                        {
                            continue;
                        }

                        clipHandler.AddClip(row, col, (T)cellValue);
                    }
                }
            }
        }

        public static void GridCopyAll<T>(GridControl grid, ClipHandler<T> clipHandler, bool excludeHeaders)
        {
            const int top = 0;
            const int left = 0;
            int right = grid.ColCount;
            int bottom = grid.RowCount;

            clipHandler.Clear();

            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    object cellValue = grid.Model[row, col].CellValue;
                    if (!(cellValue is T)) continue;
                    if (excludeHeaders && (row <= grid.Rows.HeaderCount || col <= grid.Cols.HeaderCount))
                    {
                        continue;
                    }

                    clipHandler.AddClip(row, col, (T)cellValue);
                }
            }
        }

        public static bool SelectionContainsOnlyHeadersAndScheduleDays(GridControlBase grid, GridRangeInfoList gridRangeInfoList)
        {
            if (grid == null)
                throw new ArgumentNullException("grid");

            if (gridRangeInfoList == null)
                throw new ArgumentNullException("gridRangeInfoList");

            foreach (GridRangeInfo info in gridRangeInfoList)
            {
                for (var i = info.Top; i <= info.Bottom; i++)
                {
                    for (var j = info.Left; j <= info.Right; j++)
                    {
                        if (!(grid.Model[i, j].CellValue is IScheduleDay) && grid.Model[i, j].CellType != "Header")
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Copies the selected values and headers to public clipboard.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <remarks>
        /// If you just wants an ordinary copy call gridcontrol.CutPaste.Copy()
        /// Created by: micke
        /// Created date: 2008-05-26
        /// </remarks>
        public static void CopySelectedValuesAndHeadersToPublicClipboard(GridControl control)
        {
            StringBuilder s = new StringBuilder();

            //selected range
            GridRangeInfoList rangeList = GetGridSelectedRanges(control, true);

            //loop all ranges
            foreach (GridRangeInfo range in rangeList)
            {
                //get column headers
                s.Append(GetColHeadersString(control, range));

                //loop all rows
                for (int row = range.Top; row <= range.Bottom; row++)
                {
                    //if we find a new row add newline chars
                    if (row != range.Top)
                        s.AppendLine();

                    //add rowheaderw
                    for (int i = 0; i <= control.Rows.HeaderCount; i++)
                        s.Append(string.Concat(control[row, i].FormattedText, '\t'));

                    //loop all columns
                    for (int col = range.Left; col <= range.Right; col++)
                    {
                        //if we find a new column add new column char
                        if (col != range.Left)
                            s.Append('\t');

                        //add cell value
                        s.Append(control.Model[row, col].FormattedText);
                    }
                }

                //add newline chars for each range
                s.AppendLine();
            }

            //add string to clipboard
            Clipboard.SetDataObject(new DataObject(s.ToString()), true);
        }

        private static string GetColHeadersString(GridControl control, GridRangeInfo range)
        {
            StringBuilder s = new StringBuilder();

            //loop all columns
            for (int j = 0; j <= control.Rows.HeaderCount; j++)
            {
                //add space for rowheaders
                for (int i = 0; i <= control.Rows.HeaderCount; i++)
                    s.Append("\t");

                for (int col = range.Left; col <= range.Right; col++)
                {
                    //if we find a new column add new column char
                    if (col != range.Left)
                        s.Append('\t');

                    //add header value
                    s.Append(control.Model[j, col].FormattedText);
                }

                //add new line chars
                s.AppendLine();
            }

            return s.ToString();
        }

        public static IEnumerable<IPersonWriteProtectionInfo> WriteProtectPersonSchedule(GridControl grid)
        {
            var clipHandler = new ClipHandler<ExtractedSchedule>();
            GridCopySelection(grid, clipHandler, true);
            Dictionary<IPerson, DateOnly> personAndDate = new Dictionary<IPerson, DateOnly>();
            foreach (Clip<ExtractedSchedule> clip in clipHandler.ClipList)
            {
				var person = clip.ClipValue.Person;
				var period = clip.ClipValue.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
                if (!personAndDate.ContainsKey(person))
                {
	                personAndDate.Add(person, period.EndDate);
                }
                else if (personAndDate[person] < period.StartDate)
                {
                    personAndDate[person] = period.EndDate;
                }
            }
            return changeAndPersistPeople(personAndDate);
        }

        private static IEnumerable<IPersonWriteProtectionInfo> changeAndPersistPeople(IEnumerable<KeyValuePair<IPerson, DateOnly>> personAndDates)
        {
            ICollection<IPersonWriteProtectionInfo> changedPeople = new HashSet<IPersonWriteProtectionInfo>();
            foreach (KeyValuePair<IPerson, DateOnly> pair in personAndDates)
            {
                IPerson person = pair.Key;
                changedPeople.Add(person.PersonWriteProtection);
                person.PersonWriteProtection.PersonWriteProtectedDate = pair.Value;
            }
            return changedPeople;
        }
		
        //lock selection
        public static void GridlockWriteProtected(ISchedulerStateHolder stateHolder, IGridlockManager lockManager)
        {
            lockManager.ClearWriteProtected();
	        foreach (var person in stateHolder.FilteredCombinedAgentsDictionary)
	        {
				if(person.Value.PersonWriteProtection == null) continue;
				var writeProtectUntil = person.Value.PersonWriteProtection.WriteProtectedUntil();
				if (writeProtectUntil.HasValue)
				{
					var timeZone = person.Value.PermissionInformation.DefaultTimeZone();
					var period = stateHolder.Schedules[person.Value].Period.ToDateOnlyPeriod(timeZone);
					foreach (var date in period.DayCollection())
					{
						if (writeProtectUntil.Value >= date)
						{
							lockManager.AddLock(person.Value, date, LockType.WriteProtected);
						}
					}
				}
			}
		}

        //lock selection
        public static void GridlockSelection(GridControl grid, IGridlockManager lockManager)
        {
            var clipHandler = new ClipHandler<ExtractedSchedule>();
            GridCopySelection(grid, clipHandler, true);

            foreach (Clip<ExtractedSchedule> clip in clipHandler.ClipList)
            {
                lockManager.AddLock(clip.ClipValue, LockType.Normal);
            }
        }

        public static void GridUnlockSelection(GridControl grid, IGridlockManager lockManager)
        {
            ClipHandler<ExtractedSchedule> clipHandler = new ClipHandler<ExtractedSchedule>();
            GridCopySelection(grid, clipHandler, true);

            foreach (Clip<ExtractedSchedule> clip in clipHandler.ClipList)
            {
                lockManager.RemoveLock(clip.ClipValue);
            }
        }

        public static void GridlockSpecificDayOff(GridControl grid, IGridlockManager lockManager, IDayOffTemplate dayOffTemplate)
        {
            IList<IScheduleDay> schedulesWithFreeDays = ScheduleHelper.SchedulesWithSpecificDayOff(CopyAllSchedules(grid), dayOffTemplate);
            lockManager.AddLock(schedulesWithFreeDays, LockType.Normal);

        }
        public static void GridlockFreeDays(GridControl grid, IGridlockManager lockManager)
        {
            IList<IScheduleDay> schedulesWithFreeDays = ScheduleHelper.SchedulesWithFreeDay(CopyAllSchedules(grid));
            lockManager.AddLock(schedulesWithFreeDays, LockType.Normal);

        }

        public static void GridlockAbsences(GridControl grid, IGridlockManager lockManager, IEntity absence)
        {
            IList<IScheduleDay> schedulesWithAbsence = ScheduleHelper.SchedulesWithAbsence(CopyAllSchedules(grid), absence);
            lockManager.AddLock(schedulesWithAbsence, LockType.Normal);

        }
        public static void GridlockAllAbsences(GridControl grid, IGridlockManager lockManager)
        {
            IList<IScheduleDay> schedulesWithAbsence = ScheduleHelper.SchedulesWithAbsence(CopyAllSchedules(grid));
            lockManager.AddLock(schedulesWithAbsence, LockType.Normal);

        }

        public static void GridlockShiftCategories(GridControl grid, IGridlockManager lockManager, IEntity shiftCategory)
        {
            IList<IScheduleDay> schedulesWithShiftCategory = ScheduleHelper.SchedulesWithShiftCategory(CopyAllSchedules(grid), shiftCategory);
            lockManager.AddLock(schedulesWithShiftCategory, LockType.Normal);
        }
        public static void GridlockAllShiftCategories(GridControl grid, IGridlockManager lockManager)
        {
            IList<IScheduleDay> schedulesWithShiftCategory = ScheduleHelper.SchedulesWithShiftCategory(CopyAllSchedules(grid));
            lockManager.AddLock(schedulesWithShiftCategory, LockType.Normal);
        }


        public static IList<IScheduleDay> CopyAllSchedules(GridControl grid)
        {
            IList<IScheduleDay> schedules = new List<IScheduleDay>();
            ClipHandler<ExtractedSchedule> clipHandler = new ClipHandler<ExtractedSchedule>();

            GridCopyAll(grid, clipHandler, true);

            foreach (Clip<ExtractedSchedule> clip in clipHandler.ClipList)
            {
                schedules.Add(clip.ClipValue);

            }

            return schedules;
        }

        public static IList<T> HandlePaste<T>(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction)
        {
            GridRangeInfoList rangelist = GetGridSelectedRanges(gridControl, true);
            return gridPasteAction.PasteBehavior.DoPaste(gridControl, clipHandler, gridPasteAction, rangelist);
        }

        /// <summary>
        /// Handles the paste for the scheduling grid frozen column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gridControl">The grid control.</param>
        /// <param name="clipHandler">The clip handler.</param>
        /// <param name="gridPasteAction">The grid paste action.</param>
        /// <returns></returns>
        /// <remarks>
        /// It is a bugfix for 15606: Paste rollout does not work as expected on scheduling grid
        /// </remarks>
        public static IList<T> HandlePasteScheduleGridFrozenColumn<T>(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction)
        {
            GridRangeInfoList rangelist = GetGridSelectedRanges(gridControl, true);

            GridRangeInfoList rangelistTemp = new GridRangeInfoList();

            foreach (GridRangeInfo range in rangelist)
            {
                GridRangeInfo tempRange = (GridRangeInfo)range.Clone();
                int offset = gridControl.Cols.FrozenCount - tempRange.Left;
                if (offset < 0)
                    rangelistTemp.Add(tempRange);
            }

            return gridPasteAction.PasteBehavior.DoPaste(gridControl, clipHandler, gridPasteAction, rangelistTemp);
        }

        public static ClipHandler ConvertClipboardToClipHandler()
        {
            ClipHandler clipHandler = new ClipHandler();

            //check if there is any data in clipboard
            if (Clipboard.ContainsText())
            {
	            try
	            {
		            //get text in clipboard
		            string clipboardText = Clipboard.GetText();
		            //remove "\n"
		            clipboardText = clipboardText.Replace("\n", "");
		            //remove empty string at end
		            clipboardText = clipboardText.TrimEnd();
		            //split on rows
		            string[] clipBoardRows = clipboardText.Split('\r');

		            int row = 0;
		            //loop each row
		            foreach (string rowString in clipBoardRows)
		            {
			            //split on columns
			            string[] clipBoardCols = rowString.Split('\t');

			            int col = 0;
			            //loop each column
			            foreach (string columnString in clipBoardCols)
			            {
				            //add to cliphandler
				            clipHandler.AddClip(row, col, columnString);

				            col++;
			            }

			            row++;
		            }
	            }
	            catch (OutOfMemoryException ex)
	            {
					//#39436 not crash if it's out of memory
					MessageBox.Show(ex.Message);
				}
            }

            return clipHandler;
        }

        public static ClipHandler<string> ConvertClipHandler(ClipHandler clipHandler)
        {
            ClipHandler<string> stringClipHandler = new ClipHandler<string>();
            if (clipHandler != null)
            {
                foreach (Clip clip in clipHandler.ClipList)
                {
                    stringClipHandler.AddClip(clip.RowOffset, clip.ColOffset,
                                              (clip.ClipObject == null) ? string.Empty : clip.ClipObject.ToString());
                }
            }
            return stringClipHandler;
        }

        public static ClipHandler<string> ConvertClipboardToClipHandlerString()
        {
            ClipHandler<string> clipHandler = new ClipHandler<string>();

            //check if there is any data in clipboard
            if (Clipboard.ContainsText())
            {
                //get text in clipboard
                string clipboardText = Clipboard.GetText();
                //remove "\n"
                clipboardText = clipboardText.Replace("\n", "");
                //remove empty string at end
                clipboardText = clipboardText.TrimEnd();
                //split on rows
                string[] clipBoardRows = clipboardText.Split('\r');

                int row = 0;
                //loop each row
                foreach (string rowString in clipBoardRows)
                {
                    //split on columns
                    string[] clipBoardCols = rowString.Split('\t');

                    int col = 0;
                    //loop each column
                    foreach (string columnString in clipBoardCols)
                    {
                        //add to cliphandler
                        clipHandler.AddClip(row, col, columnString);

                        col++;
                    }

                    row++;
                }
            }

            return clipHandler;
        }

        /// <summary>
        /// Fill a rounded rectangle
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="rectangle"></param>
        /// <param name="roundness"></param>
        /// <param name="brush"></param>
        /// <param name="inflate"></param>
        public static void FillRoundedRectangle(Graphics graphics, Rectangle rectangle, int roundness, Brush brush, int inflate)
        {
            Rectangle rect = rectangle;
            rect.Inflate(inflate, inflate);
            graphics.DrawPath(Pens.LightBlue, GetRoundBox(rect, roundness));
            graphics.FillPath(brush, GetRoundBox(rect, roundness));
        }

        /// <summary>
        /// Get a round box
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="roundness"></param>
        /// <returns></returns>
        public static GraphicsPath GetRoundBox(Rectangle rect, int roundness)
        {
            GraphicsPath result = new GraphicsPath();
            if (roundness > 1)
            {
                roundness = Math.Min(Math.Min(rect.Width, rect.Height) / 3, roundness);

                float diameter = roundness * 2.0F;
                SizeF sizeF = new SizeF(diameter, diameter);
                RectangleF arc = new RectangleF(rect.Location, sizeF);

                result.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                result.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                result.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                result.AddArc(arc, 90, 90);

                result.CloseFigure();
            }
            else
            {
                result.AddRectangle(rect);
            }

            return result;
        }

        /// <summary>
        /// Sorts the given data list using the given sort column in the given order.
        /// </summary>
        /// <typeparam name="T">Type of the data list</typeparam>
        /// <param name="dataList">Data list</param>
        /// <param name="sortingColumn">Sorting column</param>
        /// <param name="isAscending">Is ascending</param>
        /// <returns>Sorted data list</returns>
        public static IList<T> Sort<T>(Collection<T> dataList, string sortingColumn, bool isAscending)
        {
            // Creates the parameter for the linq expression
            var param = Expression.Parameter(typeof(T), "dataItem");
            // Creates teh linq expression requried for the sorting
            var mySortExpression =
                Expression.Lambda<Func<T, object>>(Expression.Property(param, sortingColumn), param);

            // Holds the results of the sorting process
            List<T> result;
            // Gets a iquaryale list out of the data list
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                // Sorts the quaryable list in ascending order
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                // Sorts the quaryable list in discending order
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }

            // Returns the sorted list as a collection
            return result;
        }

        #region GridStyle

        public static void GridStyle(GridControl gridControl)
        {
            gridControl.ActivateCurrentCellBehavior = GridCellActivateAction.DblClickOnCell;
            gridControl.ExcelLikeSelectionFrame = true;
            gridControl.ExcelLikeCurrentCell = true;
            gridControl.SerializeCellsBehavior = GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            gridControl.ThemesEnabled = true;
            gridControl.UseRightToLeftCompatibleTextBox = true;
            gridControl.CutPaste.ClipboardFlags &= ~GridDragDropFlags.Styles;
            gridControl.HorizontalThumbTrack = true;
            gridControl.VerticalThumbTrack = true;
            gridControl.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
            gridControl.ResizeRowsBehavior = GridResizeCellsBehavior.None;
            gridControl.Properties.MarkRowHeader = false;
            gridControl.Properties.MarkColHeader = false;
	        gridControl.Properties.BackgroundColor = SystemColors.Window;

			gridControl.GridOfficeScrollBars = OfficeScrollBars.Metro;
			gridControl.GridVisualStyles = GridVisualStyles.Metro;
			gridControl.MetroScrollBars = true;
			gridControl.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

			gridControl.BaseStylesMap.Header.StyleInfo.Font.Bold = true;
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Facename = "Segoe UI";
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Italic = false;
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Size = 8F;
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Strikeout = false;
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Underline = false;
			gridControl.BaseStylesMap.Header.StyleInfo.Font.Unit = GraphicsUnit.Point;
			gridControl.BaseStylesMap.Header.StyleInfo.TextColor = Color.Black;

			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Bold = false;
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Facename = "Segoe UI";
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Italic = false;
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Size = 8F;
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Strikeout = false;
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Underline = false;
			gridControl.BaseStylesMap.Standard.StyleInfo.Font.Unit = GraphicsUnit.Point;
			gridControl.BaseStylesMap.Standard.StyleInfo.TextColor = Color.Black;
        }

        #endregion

        #region Selection

		
        public static void HandleSelectionKeys(GridControl gridControl, KeyEventArgs e)
        {
            GridRangeInfo newGridRangeInfo = null;

            if (e.Control && e.KeyCode == Keys.A)
            {

                gridControl.BeginUpdate();
                handleSelectAll(gridControl);
                gridControl.EndUpdate();
                gridControl.Invalidate();  // refresh w/out calling handler. /kosalanp.
                e.Handled = true;
            }

            if (e.Control && e.KeyCode == Keys.Home)
            {
                newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns,
                                               gridControl.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns);
                if (newGridRangeInfo != null)
                {
                    gridControl.BeginUpdate();
                    gridControl.Selections.Clear();
                    gridControl.Selections.SelectRange(newGridRangeInfo, true);
                    gridControl.CurrentCell.MoveTo(newGridRangeInfo, GridSetCurrentCellOptions.SetFocus);
                    gridControl.EndUpdate();
                    gridControl.Invalidate();  // refresh w/out calling handler. /kosalanp.   
                    e.Handled = true;
                }
            }

            if (e.KeyCode == Keys.Tab)
            {
                //Trace.WriteLine("TAB");
                HandleTabOnGridFirstAndLastCell(gridControl, e.Shift);
            }

            if (e.Shift && e.KeyCode != Keys.ShiftKey)
            {

                switch (e.KeyCode)
                {
                    case Keys.End:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex,
                                               gridControl.CurrentCell.RowIndex, gridControl.ColCount);
                        break;
                    case Keys.Home:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, (int)ColumnType.StartScheduleColumns,
                                               gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex);

                        gridControl.CurrentCell.MoveTo(gridControl.CurrentCell.RowIndex, (int)ColumnType.StartScheduleColumns);
                        break;
                    case Keys.PageDown:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex,
                                               gridControl.RowCount, gridControl.CurrentCell.ColIndex);
                        break;
                    case Keys.PageUp:
                        newGridRangeInfo =
                            GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, gridControl.CurrentCell.ColIndex,
                                               gridControl.CurrentCell.RowIndex, gridControl.CurrentCell.ColIndex);
                        break;
                    default:
                        break;
                }
                if (newGridRangeInfo != null)
                {
                    gridControl.BeginUpdate();
                    gridControl.Selections.Clear();
                    gridControl.Selections.SelectRange(newGridRangeInfo, true);
                    gridControl.EndUpdate();
                    gridControl.Invalidate();  // refresh w/out calling handler. /kosalanp.
                    e.Handled = true;
                }
            }

            //stop move to next cell when we are at last col(arrow right)
            if (e.KeyCode == Keys.Right && gridControl.CurrentCell.ColIndex == gridControl.ColCount && gridControl.RightToLeft == RightToLeft.No)
                e.Handled = true;

            //stop move to next cell when we are att first col(arrow left)
            if (e.KeyCode == Keys.Left && gridControl.CurrentCell.ColIndex == gridControl.Cols.HeaderCount + 1 && gridControl.RightToLeft == RightToLeft.No)
                e.Handled = true;
        }


        private static void HandleTabOnGridFirstAndLastCell(GridControl gridControl, bool shift)
        {
            //get current cell
            GridCurrentCell cell = gridControl.CurrentCell;

            //check if we are on the first row, first col and shift is pressed
            if (cell.RowIndex == gridControl.Rows.HeaderCount + 1 && cell.ColIndex == gridControl.Cols.HeaderCount + 1 && shift)
            {
                Form form = gridControl.FindParentForm();
                if (!form.SelectNextControl(gridControl, false, true, false, true))
                {
                    Console.Out.WriteLine("Could not find a Control to set focus on");
                }
                return;
            }

            //check if we are on last row, last col in grid and shift is not pressed
            if (cell.RowIndex == gridControl.RowCount && cell.ColIndex == gridControl.ColCount && shift == false)
            {
                Form form = gridControl.FindParentForm();
                form.SelectNextControl(gridControl, true, true, true, true);
                return;
            }
        }

        private static void handleSelectAll(GridControl gridControl)
        {
            GridRangeInfo dataOnlyGridRangeInfo =
                GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, gridControl.Cols.HeaderCount + 1,
                                   gridControl.RowCount, gridControl.ColCount);
            GridRangeInfo dataAndHeadersGridRangeInfo =
                GridRangeInfo.Auto(0, 0, gridControl.RowCount, gridControl.ColCount);
            if (gridControl.Selections.Count == 1)
            {
                GridRangeInfo existingGridRangeInfo = gridControl.Selections.Ranges[0];
                gridControl.Selections.Clear();

                if (existingGridRangeInfo == dataOnlyGridRangeInfo)
                {
                    gridControl.Selections.Add(dataAndHeadersGridRangeInfo);
                }
                else if (existingGridRangeInfo != dataOnlyGridRangeInfo && existingGridRangeInfo != dataAndHeadersGridRangeInfo)
                {
                    gridControl.Selections.Add(dataOnlyGridRangeInfo);
                }
            }
            else
            {
                gridControl.Selections.Clear();
                gridControl.Selections.Add(dataOnlyGridRangeInfo);
            }
        }

		public static void HandleSelectAllSchedulingView(GridControl gridControl)
		{
			var dataOnlyGridRangeInfo = GridRangeInfo.Auto(gridControl.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns, gridControl.RowCount, gridControl.ColCount);
			var dataAndHeadersGridRangeInfo = GridRangeInfo.Auto(0, 0, gridControl.RowCount, gridControl.ColCount);
			if (gridControl.Selections.Count == 1)
			{
				var existingGridRangeInfo = gridControl.Selections.Ranges[0];
				gridControl.Selections.Clear();

				if (existingGridRangeInfo == dataOnlyGridRangeInfo)
					gridControl.Selections.Add(dataAndHeadersGridRangeInfo);

				else if (existingGridRangeInfo != dataOnlyGridRangeInfo && existingGridRangeInfo != dataAndHeadersGridRangeInfo)
					gridControl.Selections.Add(dataOnlyGridRangeInfo);
			}
			else
			{
				gridControl.Selections.Clear();
				gridControl.Selections.Add(dataOnlyGridRangeInfo);
			}
		}


        #endregion

        /// <summary>
        /// return a list with meetings for selected area
        /// </summary>
        /// <returns></returns>
        public static IList<IPersonMeeting> MeetingsFromSelection(GridControl grid)
        {
            IList<IPersonMeeting> personMeetings = new List<IPersonMeeting>();

            var clipHandler = new ClipHandler<ExtractedSchedule>();
            GridCopySelection(grid, clipHandler, true);

            foreach (Clip<ExtractedSchedule> clip in clipHandler.ClipList)
            {
                IPersonMeeting selectedPersonMeeting = null;

                var personMeetingsFromClip = clip.ClipValue.PersonMeetingCollection();
                if (personMeetingsFromClip.Length > 0)
                {
                    if (personMeetingsFromClip.Length > 1)
                    {
                        IMeeting selectedMeeting;
                        using (MeetingPicker meetingPicker = new MeetingPicker(clip.ClipValue.Person.Name + " " +
                            clip.ClipValue.Period.StartDateTimeLocal(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone).ToShortDateString(), personMeetingsFromClip))
                        {
                            meetingPicker.ShowDialog();
                            selectedMeeting = meetingPicker.SelectedMeeting();
                        }

                        foreach (IPersonMeeting personMeeting in personMeetingsFromClip)
                        {
                            if (personMeeting.BelongsToMeeting.Equals(selectedMeeting))
                            {
                                selectedPersonMeeting = personMeeting;
                                break;
                            }
                        }
                    }
                    else
                    {
                        selectedPersonMeeting = personMeetingsFromClip[0];
                    }
                }

                if (selectedPersonMeeting != null)
                    personMeetings.Add(selectedPersonMeeting);
            }

            return personMeetings;
        }

        public static void SelectRangeOnGrid(GridControlBase grid, GridRangeInfo range)
        {
            InParameter.NotNull("grid", grid);
            grid.BeginUpdate();
            grid.Model.Selections.Clear(true);
            grid.Model.Selections.SelectRange(range, true);
            grid.CurrentCell.MoveTo(range, GridSetCurrentCellOptions.SetFocus);
            grid.EndUpdate();
            grid.Select();
        }

        public static void SelectFirstRowOnGrid(GridControl grid)
        {
            InParameter.NotNull("grid", grid);
            GridRangeInfo gridRangeInfo = GridRangeInfo.Row(grid.Rows.HeaderCount + 1);
            SelectRangeOnGrid(grid, gridRangeInfo);
        }
    }

    /// <summary>
    /// Represents extended text paste action class
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/17/2008
    /// </remarks>
    public class ExtendedTextPasteAction : IGridPasteAction<string>
    {

        #region IGridPasteAction<string> Members

        public string Paste(GridControl gridControl, Clip<string> clip, int rowIndex, int columnIndex)
        {
            string returnValue = string.Empty;
            try
            {
                gridControl.Model[rowIndex, columnIndex].ApplyFormattedText(clip.ClipValue);
                returnValue = clip.ClipValue;
            }
            catch (FormatException)
            {
                returnValue = string.Empty;
            }
            catch (InvalidCastException)
            {
                returnValue = string.Empty;
            }
            catch (ArgumentException)
            {
                returnValue = string.Empty;
            }
            catch (Exception exp)
            {
                if (exp.InnerException.GetType() == typeof (FormatException))
                {
                    returnValue = string.Empty;
                }
            }

            return returnValue;
        }

        public IPasteBehavior PasteBehavior
        {
            get { return new ExtenderPasteBehavior(); }
        }

		public PasteOptions PasteOptions
		{
			get { return new PasteOptions(); }
		}

        #endregion
    }

    class SimpleTextPasteAction : IGridPasteAction<string>
    {
        #region IGridPasteAction<string> Members
        public IPasteBehavior PasteBehavior
        {
            get { return new NormalPasteBehavior(); }
        }

		public PasteOptions PasteOptions
		{
			get { return new PasteOptions(); }
		}

        public string Paste(GridControl gridControl, Clip<string> clip, int rowIndex, int columnIndex)
        {
            try
            {
                gridControl.Model[rowIndex, columnIndex].ApplyFormattedText(clip.ClipValue);
                return clip.ClipValue;
            }
            //TODO:Need to refactor
            catch (FormatException)
            {
                return null;
            }
            catch (InvalidCastException)
            { return null; }
        }

        #endregion


    }
}
