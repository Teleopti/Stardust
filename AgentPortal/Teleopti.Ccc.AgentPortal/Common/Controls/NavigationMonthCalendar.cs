using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class NavigationMonthCalendar : Panel, ISupportInitialize
    {
        #region Fields
        private ScheduleAppearance _appearance;
        private const int ButtonDx = 4;
        private const int ButtonHeight = 8;
        private const int ButtonXOffset = 6;
        private GridControl _calendar;
        //private int _correction = 15;
        private DateTime _dateValue = DateTime.Today;
        //private int _dXPad = 2;
        //private int _dYPad = 2;
        private int _firstRowAdjustment;
        private const int HeaderHeight = 0x10;
        private bool _keepFullRows;
        private DateTime _lastMouseDownDate = DateTime.MinValue;
        private int _numberCalendars = 3;
        private const int Pad = 12;
        private int _paintFrozen;
        private const int RowsPerCal = 8;
        private DateSelections _selectedDates;
        //private bool _showWeekNumbers = true;
        private int _targetHeight;
        private int _targetWidth;
        private DateTime _today = DateTime.Today;
        private ScheduleViewType _scheduleType = ScheduleViewType.Day;
        private readonly IList<DayOfWeek> _weekdays;
        private Interfaces.Domain.MinMax<DateTime> _allowedDateInterval = new Interfaces.Domain.MinMax<DateTime>(DateTime.MinValue, DateTime.MaxValue);

        private int _fastNavigationSteps = 12;
        private const int WeekNumberIndent = 12;
        private const int WmSetredraw = 11;

        #endregion

        public event DateValueChangedEventHandler DateValueChanged;
        public event DateValueChangingEventHandler DateValueChanging;

        #region Default values
        private void InitializeComponent()
        {
            if (!DesignMode)
            {
                _calendar = CreateCalendarGrid();
                _calendar.BeginInit();
                SuspendLayout();
                _calendar.TableStyle.CultureInfo = CultureInfo.CurrentUICulture;
                _calendar.BaseStylesMap["Standard"].StyleInfo.Font.Facename = "Arial";
                _calendar.BaseStylesMap["Standard"].StyleInfo.Font.Size = 8;

                _calendar.HScrollBehavior = GridScrollbarMode.Disabled;
                _calendar.VScrollBehavior = GridScrollbarMode.Disabled;
                _calendar.RowCount = (_numberCalendars * RowsPerCal) - 1;
                _calendar.ColCount = 8;
                _calendar.Cols.Hidden[0] = true;
                _calendar.AllowSelection = GridSelectionFlags.AlphaBlend | GridSelectionFlags.Keyboard |
                                           GridSelectionFlags.Shift | GridSelectionFlags.Multiple |
                                           GridSelectionFlags.Cell;
                _calendar.Model.Options.ResizeColsBehavior = GridResizeCellsBehavior.None;
                _calendar.ControllerOptions &= ~GridControllerOptions.OleDataSource;
                _calendar.ActivateCurrentCellBehavior = GridCellActivateAction.None;
                _calendar.Model.Options.ShowCurrentCellBorderBehavior = GridShowCurrentCellBorder.HideAlways;
                _calendar.DefaultRowHeight = 15;
                _calendar.SmoothControlResize = false;
                _calendar.AlphaBlendSelectionColor = Color.FromArgb(0, 0, 0, 0);
                Appearance.NavigationCalendarHeaderColor = Color.FromArgb(196, 221, 255);
                Appearance.NavigationCalendarArrowColor = Color.FromArgb(101, 147, 208);
                using (Graphics graphics = _calendar.CreateGraphics())
                {
                    _targetWidth = ((int) (15f * graphics.MeasureString("8", _calendar.Font).Width)) + (2 * Pad);
                    _targetHeight = _calendar.Model.RowHeights.GetTotal(0, _calendar.RowCount);
                }
                _calendar.Dock = DockStyle.None;
                

                _calendar.Size = new Size(_targetWidth, _targetHeight);
                _calendar.Name = "_calendar";
                _calendar.CellDrawn += GridCellDrawn;
                _calendar.Model.SelectionChanging += ModelSelectionChanging;
                _calendar.Model.SelectionChanged += ModelSelectionChanged;
                _calendar.QueryCoveredRange += CalendarQueryCoveredRange;
                _calendar.QueryRowHeight += CalendarQueryRowHeight;
                _calendar.QueryColWidth += CalendarQueryColWidth;
                _calendar.QueryCellInfo += CalendarQueryCellInfo;
                _calendar.MouseDown += CalendarMouseDown;
                _calendar.DrawCellDisplayText += CalendarDrawCellDisplayText;
                _calendar.Model.Options.DefaultGridBorderStyle = GridBorderStyle.None;
                _calendar.TableStyle.HorizontalAlignment = GridHorizontalAlignment.Center;
                _calendar.TableStyle.TextMargins.Left = 0;
                _calendar.TableStyle.TextMargins.Right = 0;
                //this._calendar.RightToLeft = this.RightToLeft;
                _calendar.RightToLeft = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
                                            ? RightToLeft.Yes
                                            : RightToLeft.No;

                //Hmm... problems, just patching this calander with duck tape, dont like this at all
                _calendar.Location = _calendar.RightToLeft == RightToLeft.Yes ? new Point(4, 2) : new Point(10, 2);

                Controls.Add(_calendar);
                _calendar.TabIndex = 0;
                
                
                
                _calendar.EndInit();
            }
            ResumeLayout(false);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationMonthCalendar"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public NavigationMonthCalendar()
        {
            InitializeComponent();
            SetCurrentPersonCulture();
            _weekdays = DateHelper.GetDaysOfWeek(CurrentCulture);
            
            if (Helper.HelperFunctions.RuntimeMode())
            {
                _calendar.Paint += CalendarPaint;
            }
        }

        
        public void SetCurrentPersonCulture()
        {
            CultureInfo uiCulture = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (Helper.HelperFunctions.RuntimeMode())
            {
                PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
                if (person.UICultureLanguageId.HasValue)
                {
                    uiCulture = CultureInfo.GetCultureInfo(person.UICultureLanguageId.Value);
                }
                if (person.CultureLanguageId.HasValue)
                {
                    culture = CultureInfo.GetCultureInfo(person.CultureLanguageId.Value);
                }
            }
			CurrentUICulture = uiCulture.FixPersianCulture();
			CurrentCulture = culture.FixPersianCulture();
            //AddWeekDays(DateHelper.GetDaysOfWeek(culture));
        }
        /// <summary>
        /// Adjusts the selections by month.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public void AdjustSelectionsByMonth(int offset)
        {
            if (offset == 0) return;
            if (offset > int.MaxValue) throw new ArgumentOutOfRangeException("offset");

            switch (_scheduleType)
            {
                case ScheduleViewType.Week:
                    for (int i = 0; i < SelectedDates.Count; i++)
                    {
                        SelectedDates[i] = SelectedDates[i].AddDays((offset * 0x1c));
                    }
                    break;
                default:
                    for (int j = 0; j < SelectedDates.Count; j++)
                    {
                        SelectedDates[j] = CurrentCalendar.AddMonths(SelectedDates[j], offset);
                    }
                    break;
            }
            SelectedDates.OnSelectionsChanged();
        }

        /// <summary>
        /// Signals the object that initialization is starting.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public void BeginInit()
        {
        }

        /// <summary>
        /// Begins the update.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        public void BeginUpdate()
        {
            if (!FreezePainting)
            {
                FreezePainting = true;
            }
        }

        private void CalendarDrawCellDisplayText(object sender, GridDrawCellDisplayTextEventArgs e)
        {
            if (IsDateGridCell(e.RowIndex, e.ColIndex))
            {
                int months = e.RowIndex / RowsPerCal;
                var cellValue = (DateTime)e.Style.CellValue;
                var time2 = CurrentCalendar.AddMonths(_dateValue, months);
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                if (IsDateOnSameMonth(months, cellValue, time2))
                {
                    e.DisplayText = " ";
                }
                else
                {
                    e.DisplayText = CurrentCalendar.GetDayOfMonth(cellValue).ToString(CurrentCulture);
                    e.Style.Font.Size = 8;
                }
                
            }
           
            //e.Style.Font.Size = 6;
            //e.DisplayText = e.RowIndex + " " + e.ColIndex;
        }

        //Extracted this to its own meth, 
        private bool IsDateOnSameMonth(int months, DateTime cellValue, DateTime time2)
        {
            return (((months == 0) && (GetYearMonthValue(cellValue) > GetYearMonthValue(time2))) ||
                    ((months > 0) && (months < (_numberCalendars - 1)) && (GetYearMonthValue(cellValue) != GetYearMonthValue(time2)))) ||
                   ((months == (_numberCalendars - 1)) && (GetYearMonthValue(cellValue) < GetYearMonthValue(time2)));
        }

        /// <summary>
        /// Gets the year month value.
        /// Returns year times hundred plus month
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        private int GetYearMonthValue(DateTime theDate)
        {
            return ((100 * CurrentCalendar.GetYear(theDate)) +
                    CurrentCalendar.GetMonth(theDate));
        }

        private void CalendarMouseDown(object sender, MouseEventArgs e)
        {
            int colIndex, rowIndex;
            var point = new Point(e.X, e.Y);

            if (_calendar.PointToRowCol(point, out rowIndex, out colIndex) &&
                (rowIndex == 0))
            {
                int height = _calendar.RowHeights[rowIndex];
                int width = _calendar.ColWidths.GetTotal(0, _calendar.ColCount);
                const int doubleArrowWidth = (int)(2.5d * ButtonDx);

                //CreateProjection for rectangles to test for
                var rect1 = new Rectangle(ButtonXOffset, 0, doubleArrowWidth, height);
                var rect2 = rect1;
                rect2.X = rect1.Right;
                rect2.Width = doubleArrowWidth - ButtonDx;

                var rect4 = new Rectangle(width - doubleArrowWidth - ButtonXOffset, 0, doubleArrowWidth, height);
                Rectangle rect3 = rect4;
                rect3.X -= doubleArrowWidth - ButtonDx;
                rect3.Width = doubleArrowWidth - ButtonDx;

                if (rect2.Contains(point)) DateValue = CurrentCalendar.AddMonths(DateValue, -1);
                else if (rect3.Contains(point)) DateValue = CurrentCalendar.AddMonths(DateValue, 1);
                else if (rect1.Contains(point)) DateValue = CurrentCalendar.AddMonths(DateValue, -_fastNavigationSteps);
                else if (rect4.Contains(point)) DateValue = CurrentCalendar.AddMonths(DateValue, _fastNavigationSteps);

                _calendar.Refresh();
                _calendar.Invalidate();
                _calendar.Update();
            }
        }

        private void CalendarPaint(object sender, PaintEventArgs e)
        {
            if (Appearance != null)
            {
                _calendar.Paint -= CalendarPaint;
                _calendar.Properties.BackgroundColor = Appearance.NavigationCalendarBackColor;
                _calendar.Invalidate();
                BackColor = Appearance.NavigationCalendarBackColor;
            }
        }

        private void CalendarQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColIndex < 0) return;

            int rest = e.RowIndex % RowsPerCal;
            int padding = (rest > 0) ? Pad : 0;

            e.Style.BackColor = Appearance.NavigationCalendarBackColor;
            e.Style.TextColor = Appearance.NavigationCalendarTextColor;

            switch (e.ColIndex)
            {
                case 1:
                    e.Style.TextMargins.Left = padding;
                    if (rest == 0)
                    {
                        int months = e.RowIndex / RowsPerCal;
                        DateTime time = CurrentCalendar.AddMonths(_dateValue, months);
                        WriteMonthYearHeader(e.Style, time);
                    }
                    
                    break;
                case 8:
                case 9:
                    //e.Style.TextMargins.Right = padding;
                    break;
            }

            //Writes the weeknumber
            if(e.ColIndex > 0 && (rest > 1))
            {
                writeWeekNumber(e);
            }

            //This writes the Short day names
            if ((e.ColIndex > 1) && (rest == 1))
            {
                writeShortDayNames(e);
            }
            //This writes the day
            else if ((rest > 1) && (e.ColIndex > 1))
            {
                writeDay(e);
            }
        }

        //I just moved these to their own methods, dont understand this spagetti-code
        #region sri-lanki-spagetthi
        private void writeDay(GridQueryCellInfoEventArgs e)
        {
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            DateTime time2;
            int monthsToAdd = e.RowIndex / RowsPerCal;
            e.Style.CellValue = time2 = GetCellValue(e.RowIndex, e.ColIndex);
            int month = CurrentCalendar.GetMonth(CurrentCalendar.AddMonths(_dateValue, monthsToAdd));
            if (CurrentCalendar.GetMonth(time2) != month || !IsDateAllowed(time2))
            {
                e.Style.TextColor = Appearance.NavigationCalendarDisabledTextColor;
            }
            if ((/*!ShowWeekNumbers || */(e.ColIndex > 1)) && (SelectedDates.IndexOf(time2) > -1))
            {
                DateTime time3 = CurrentCalendar.AddMonths(_dateValue, monthsToAdd);
                if ((((monthsToAdd != 0) || (GetYearMonthValue(time2) <= GetYearMonthValue(time3))) &&
                     (((monthsToAdd <= 0) || (monthsToAdd >= (_numberCalendars - 1))) || (GetYearMonthValue(time2) == GetYearMonthValue(time3)))) &&
                    ((monthsToAdd != (_numberCalendars - 1)) || (GetYearMonthValue(time2) >= GetYearMonthValue(time3))))
                {
                    e.Style.BackColor = Appearance.NavigationCalendarSelectionColor;
                }
            }
        }

        private void writeShortDayNames(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellValue = CurrentUICulture.DateTimeFormat.GetShortestDayName(_weekdays[e.ColIndex - 2])[0];
            e.Style.Borders.Bottom = e.ColIndex == 1 ? new GridBorder(GridBorderStyle.None) : new GridBorder(GridBorderStyle.Solid, Appearance.NavigationCalendarHeaderColor);
        }

        private void writeWeekNumber(GridQueryCellInfoEventArgs e)
        {
            DateTime time = GetCellValue(e.RowIndex, 2);

            int months = e.RowIndex / RowsPerCal;
            DateTime time2 = CurrentCalendar.AddMonths(_dateValue, months);
            if(!IsDateOnSameMonth(months,time,time2))
            {
                int weekOfYear = DateHelper.WeekNumber(time, CurrentCulture);
                e.Style.Font.Size = 6;
                e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Left;
                e.Style.CellValue = weekOfYear;
                e.Style.TextMargins.Left = 0;
            }
                
            e.Style.Borders.Right = e.ColIndex == 1 ? new GridBorder(GridBorderStyle.Solid, Appearance.NavigationCalendarHeaderColor) : new GridBorder(GridBorderStyle.None);
        }
        
        #endregion 

        private bool IsDateAllowed(DateTime theDate)
        {
            return (theDate.Date >= _allowedDateInterval.Minimum.Date &&
                    theDate.Date <= _allowedDateInterval.Maximum.Date);
        }

        private void WriteMonthYearHeader(GridStyleInfo styleInfo, DateTime time)
        {
            styleInfo.BackColor = Appearance.NavigationCalendarHeaderColor;
            styleInfo.Text = time.AddDays((-CurrentCalendar.GetDayOfMonth(time) + 1)).ToString("MMMM yyyy", CurrentUICulture);
            styleInfo.VerticalAlignment = GridVerticalAlignment.Middle;
            styleInfo.CellType = "Static";
            styleInfo.Font.Bold = false;
        }

        private Calendar CurrentCalendar
        {
            get { return CurrentCulture.Calendar; }
        }

        public CultureInfo CurrentCulture { get; set; }

        public CultureInfo CurrentUICulture { get; set; }

        private void CalendarQueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index <= 0) return;
            
            int num = (_calendar.Width - (2 * Pad)) / 8;

            //This date control has huge problems, had to brake into two methods to make RTL to work?!?!
            if (_calendar.RightToLeft == RightToLeft.Yes)
                SizeRightToLeft(e, num);
            else
                SizeLeftToRight(e, num);

            e.Handled = true;
        }

        private static void SizeRightToLeft(GridRowColSizeEventArgs e, int num)
        {
            switch (e.Index)
            {
                case 8:
                    e.Size = num + 10;
                    break;
                case 0:
                    e.Size = num - 2;
                    break;
                case 1:
                    e.Size = num - 4;
                    break;
                default:
                    e.Size = num + 3;
                    break;
            }
        }

        private static void SizeLeftToRight(GridRowColSizeEventArgs e, int num)
        {
            switch (e.Index)
            {
                case 0:
                    e.Size = num - 2;
                    break;
                case 1:
                    e.Size = num - 4;
                    break;
                default:
                    e.Size = num + 2;
                    break;
            }
        }
        
        private static void CalendarQueryCoveredRange(object sender, GridQueryCoveredRangeEventArgs e)
        {
            if ((e.RowIndex % RowsPerCal) == 0)
            {
                e.Range = GridRangeInfo.Cells(e.RowIndex, 1, e.RowIndex, 8);
                e.Handled = true;
            }
        }

        private static void CalendarQueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            if ((e.Index % RowsPerCal) == 0)
            {
                e.Size = HeaderHeight;
                e.Handled = true;
            }
        }

        private static GridControl CreateCalendarGrid()
        {
            return new GridControl();
        }

        public void EndInit()
        {
        }

        public void EndUpdate()
        {
            if (FreezePainting)
            {
                FreezePainting = false;
            }
        }

        private DateTime GetCellValue(int row, int col)
        {
            int months = row / RowsPerCal;
            var offset = ((int)CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            DateTime localDateValue;

            switch (months)
            {
                case 0:
                    {
                        localDateValue = _dateValue.AddDays((-CurrentCalendar.GetDayOfMonth(_dateValue) + 1));
                        offset -= (int)localDateValue.DayOfWeek;
                        if (offset > 0) offset -= 7;

                        int num2 = (((((row - 2) * 7) + col - 2) + offset)) + _firstRowAdjustment;
                        if ((num2 <= 0) && DesignMode)
                        {
                            return localDateValue;
                        }
                        return localDateValue.AddDays(num2);
                    }
            }
            localDateValue = CurrentCalendar.AddMonths(_dateValue, months);
            localDateValue = localDateValue.AddDays((-CurrentCalendar.GetDayOfMonth(localDateValue) + 1));

            offset -= (int)localDateValue.DayOfWeek;
            if (offset > 0) offset -= 7;

            return localDateValue.AddDays(((((((row - 2) - (months * RowsPerCal)) * 7) + col - 2) + offset)));
        }

        private void GridCellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (!(sender is GridControl)) return;

            if (e.RowIndex == 0)
            {
                #region Draw the arrows for month browsing
                using (Brush brush = new SolidBrush(Appearance.NavigationCalendarArrowColor))
                {
                    #region Explanation
                    //
                    //     p1
                    //   p2
                    //     p3
                    //
                    // p1 = Point(x+bDX, y)
                    // p2 = Point(x, y+bhh) (buttonHeightHalf)
                    // p3 = Point(x+bDX, y+bh) (_buttonHeight)
                    #endregion
                    int num = Math.Max(1, (e.Bounds.Height - ButtonHeight) / 2);
                    //int p2_x = e.Bounds.Location.X + _buttonXOffset;
                    int p1Y = e.Bounds.Location.Y + num;

                    //CreateProjection month and fast nav arrows
                    int p2X = e.Bounds.Location.X + ButtonXOffset;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, true);
                    p2X += ButtonDx;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, true);

                    p2X += ButtonDx * 2;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, true);

                    //CreateProjection mirrored arrow on other side (by moving and reversing x coordinates
                    p2X = (e.Bounds.Right - ButtonXOffset) - ButtonDx;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, false);

                    p2X -= ButtonDx;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, false);

                    p2X -= ButtonDx * 2;
                    DrawArrow(e.Graphics, brush, p2X, p1Y, false);
                }
                #endregion
                return;
            }

            if (!IsDateGridCell(e.RowIndex, e.ColIndex)) return;

            if (e.Style.CellValue.Equals(string.Empty)) return;

            var cellValue = (DateTime)e.Style.CellValue;
            if (cellValue.Date == Today.Date)
            {
                int months = e.RowIndex / RowsPerCal;
                if (CurrentCalendar.GetMonth(cellValue) == CurrentCalendar.GetMonth(CurrentCalendar.AddMonths(_dateValue, months)))
                {
                    using (var pen = new Pen(Appearance.NavigationCalendarTodayColor))
                    {
                        Rectangle bounds = e.Bounds;
                        bounds.Inflate(-1, -1);
                        bounds.Width--;
                        e.Graphics.DrawRectangle(pen, bounds);
                    }
                }
            }
        }

        private static void DrawArrow(Graphics graphics, Brush brush, int p2X, int p1Y, bool pointToLeft)
        {
            var points = new[] { 
                                             new Point(p2X, p1Y), 
                                             new Point(p2X + ButtonDx, p1Y + (ButtonHeight / 2)), 
                                             new Point(p2X, p1Y + ButtonHeight), 
                                             new Point(p2X, p1Y) };

            if (pointToLeft)
            {
                points[0].X += ButtonDx;
                points[1].X -= ButtonDx;
                points[2].X += ButtonDx;
                points[3].X += ButtonDx;
            }

            graphics.FillPolygon(brush, points);
        }


        //Arghh crap
        internal static bool IsDateGridCell(int row, int col)
        {
            if(col == 1) return false;
           // if (col == 8) return false;// && (row % rowsPerCal) != 1) return false;
            if(!(((row % RowsPerCal) > 1) && (col > 1))) return false;

            return ((((col != 1) || ((row % RowsPerCal) != 0)) &&
                     ((col <= 0) || ((row % RowsPerCal) != 1))) && (row > 1));
        }

        private void ModelSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            int rowIndex, colIndex;
            if ((((e.Reason != GridSelectionReason.MouseDown) && (e.Range != null)) && (_lastMouseDownDate != DateTime.MinValue)) && _calendar.PointToRowCol(_calendar.PointToClient(MousePosition), out rowIndex, out colIndex))
            {
                DateTime cellValue = GetCellValue(rowIndex, colIndex);
                if (SelectedDates.Count > 0)
                {
                    bool flag = false;
                    var timeArray = new DateSelections();
                    while (cellValue < _lastMouseDownDate)
                    {
                        timeArray.Add(cellValue);
                        flag = true;
                        cellValue = cellValue.AddDays(1.0);
                    }
                    while (cellValue >= _lastMouseDownDate)
                    {
                        timeArray.Add(cellValue);
                        flag = true;
                        cellValue = cellValue.AddDays(-1.0);
                    }
                    if (ControlKeyDown)
                    {
                        foreach (DateTime time2 in timeArray)
                        {
                            if (!SelectedDates.Contains(time2))
                            {
                                SelectedDates.Add(time2);
                            }
                        }
                    }
                    else
                    {
                        SelectedDates.BeginUpdate();
                        SelectedDates.Clear();
                        SelectedDates.AddRange(timeArray);
                        SelectedDates.EndUpdate();
                    }
                    SelectedDates.Sort();
                    int count = SelectedDates.Count;
                    DateTime t = SelectedDates[0];
                    DateTime time4 = SelectedDates[count - 1];
                    //(this.SelectedDates.Count > this.scheduleGridGrid.Schedule.Appearance.DayMonthCutoff) || 
                    if ((_keepFullRows) && ((!ControlKeyDown && (ScheduleType != ScheduleViewType.Month)) && (ScheduleType != ScheduleViewType.Week)))
                    {
                        DayOfWeek firstDayOfWeek = CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                        var lastWeekday = (int)firstDayOfWeek;
                        if (lastWeekday < 0) lastWeekday += 7;
                        var lastDayOfWeek = (DayOfWeek)lastWeekday;

                        _keepFullRows = true;
                        while (t.DayOfWeek != firstDayOfWeek)
                        {
                            t = t.AddDays(-1.0);
                            SelectedDates.Add(t);
                            flag = true;
                        }
                        while (time4.DayOfWeek != lastDayOfWeek)
                        {
                            time4 = time4.AddDays(1.0);
                            SelectedDates.Add(time4);
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        _calendar.Invalidate();
                        _calendar.Update();
                    }
                }
            }
        }

        private void ModelSelectionChanging(object sender, GridSelectionChangingEventArgs e)
        {
            if ((e.Reason == GridSelectionReason.MouseDown) && (((e.ClickRange == null) || e.ClickRange.IsEmpty) || ControlKeyDown))
            {
                int rowIndex, colIndex;
                Point point = _calendar.PointToClient(MousePosition);
                if (_calendar.PointToRowCol(point, out rowIndex, out colIndex) && ((/*!ShowWeekNumbers || */(colIndex != 1)) || (point.X >= WeekNumberIndent)))
                {
                    SelectedDates.BeginUpdate();
                    if ((rowIndex % RowsPerCal) > 1)
                    {
                        if (!ControlKeyDown && (SelectedDates.Count > 0))
                        {
                            SelectedDates.Clear();
                            _calendar.Invalidate();
                            _calendar.Update();
                        }
                        DateTime cellValue = GetCellValue(rowIndex, colIndex);
                        _lastMouseDownDate = cellValue;
                        _keepFullRows = false;
                        if (SelectedDates.IndexOf(cellValue) == -1)
                        {
                            SelectedDates.Add(cellValue);
                        }
                    }
                    SelectedDates.EndUpdate();
                }
            }
        }

        protected virtual void OnDateValueChanged()
        {
            if (DateValueChanged != null)
            {
                var e = new EventArgs();
                DateValueChanged(this, e);
            }
        }

        protected virtual bool OnDateValueChanging(DateTime oldValue, DateTime newValue)
        {
            var e = new DateValueEventArgs(oldValue, newValue);
            if (DateValueChanging != null)
            {
                DateValueChanging(this, e);
            }
            return e.Cancel;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
           // _calendar.Location = new Point(Math.Max(_dXPad, ((base.Width - _targetWidth) + _correction) / 2), _dYPad);
            int numberOfCalendars = Height / (_calendar.Model.RowHeights.GetTotal(0, 7) + 1);
            if ((numberOfCalendars > 1) && (numberOfCalendars != _numberCalendars))
            {
                _numberCalendars = numberOfCalendars;
                _calendar.RowCount = (_numberCalendars * RowsPerCal) - 1;
                _targetHeight = _calendar.Model.RowHeights.GetTotal(0, _calendar.RowCount);
            }
            _calendar.Size = new Size(_targetWidth, _targetHeight);
        }

        private void SendMessage(int msg, int wparam, int lparam)
        {
            UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), msg, (IntPtr)wparam, (IntPtr)lparam);
        }

        // Properties
        private ScheduleAppearance Appearance
        {
            get { return _appearance ?? (_appearance = new ScheduleAppearance(null)); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        private DateTime BottomRightDate
        {
            get
            {
                return CurrentCalendar.AddMonths(DateValue, _numberCalendars).AddDays(-CurrentCalendar.GetDayOfMonth(DateValue));
            }
        }

        [Browsable(false)]
        public GridControl CalendarGrid
        {
            get
            {
                return _calendar;
            }
        }

        private static bool ControlKeyDown
        {
            get
            {
                if ((ModifierKeys & Keys.Control) == Keys.None)
                {
                    return ((ModifierKeys & Keys.Shift) != Keys.None);
                }
                return true;
            }
        }

        public DateTime DateValue
        {
            get
            {
                return _dateValue;
            }
            set
            {
                if ((_dateValue != value) && !OnDateValueChanging(_dateValue, value))
                {
                    BeginUpdate();
                    _dateValue = value;
                    DayOfWeek firstDayOfWeek = CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    _firstRowAdjustment =
                        (_dateValue.AddDays((-CurrentCalendar.GetDayOfMonth(_dateValue) + 1)).DayOfWeek == firstDayOfWeek)
                            ? -7 : 0;

                    if (_firstRowAdjustment < 0 &&
                        _firstRowAdjustment + (int)firstDayOfWeek + 1 >= 0)
                        _firstRowAdjustment = 0;

                    if (SelectedDates.Count > 0)
                    {
                        if (SelectedDates[0] < _dateValue.AddDays((-CurrentCalendar.GetDayOfMonth(_dateValue) + 1)))
                        {
                            AdjustSelectionsByMonth(1);
                        }
                        else if (SelectedDates[SelectedDates.Count - 1] > BottomRightDate)
                        {
                            AdjustSelectionsByMonth(-1);
                        }
                    }
                    EndUpdate();
                    OnDateValueChanged();
                }
            }
        }

        private bool FreezePainting
        {
            get
            {
                return (_paintFrozen > 0);
            }
            set
            {
                if ((value && IsHandleCreated) && (Visible && (_paintFrozen++ == 0)))
                {
                    SendMessage(WmSetredraw, 0, 0);
                }
                if ((!value && (_paintFrozen != 0)) && (--_paintFrozen == 0))
                {
                    SendMessage(WmSetredraw, 1, 0);
                    Invalidate(true);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DateSelections SelectedDates
        {
            get { return _selectedDates ?? (_selectedDates = new DateSelections()); }
        }

        /// <summary>
        /// Gets or sets the _today.
        /// </summary>
        /// <value>The _today.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Today
        {
            get
            {
                return _today;
            }
            set
            {
                _today = value;
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the type of the schedule.
        /// </summary>
        /// <value>The type of the schedule.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        [Browsable(true)]
        public ScheduleViewType ScheduleType
        {
            get { return _scheduleType; }
            set { _scheduleType = value; }
        }

        /// <summary>
        /// Gets or sets the type of the schedule.
        /// </summary>
        /// <value>The type of the schedule.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-03
        /// </remarks>
        [Browsable(true), DefaultValue(12)]
        public int FastNavigationSteps
        {
            get { return _fastNavigationSteps; }
            set { _fastNavigationSteps = value; }
        }

        /// <summary>
        /// Gets or sets the allowed date interval.
        /// </summary>
        /// <value>The allowed date interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        //[Browsable(false)]
        public Interfaces.Domain.MinMax<DateTime> AllowedDateInterval
        {
            get { return _allowedDateInterval; }
            set { _allowedDateInterval = value; }
        }
    }

    /// <summary>
    /// Static class for calls to unsafe native methods
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-03
    /// </remarks>
    internal static class UnsafeNativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}