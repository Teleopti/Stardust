using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Schedule;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortal.Properties;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public class CustomScheduleGrid : ScheduleGrid
    {
        private Color _allDayColor;
        private const int _roundedCornersRadius = 5;
        private const HatchStyle _requestHatchStyle = HatchStyle.WideDownwardDiagonal;
        private const HatchStyle _restrictionHatchStyle = HatchStyle.WideUpwardDiagonal;
        private const HatchStyle _personMeetingHatchStyle = HatchStyle.Weave;
        private const HatchStyle _personalShiftHatchStyle = HatchStyle.Wave;

        private readonly int AllDayRow = 1;
        private readonly int DayColCount = 32;
        private readonly int MarkColumn = 2;
        private readonly string DATEFORMAT_WEEK_MONTH_NEWMONTH = "MMMM d";
        private readonly string DATEFORMAT_WEEK_MONTH_NEWYEAR = "MMMM d, yyyy";

        private readonly IList<GridStyleInfo> _selectedCellStyleList = new List<GridStyleInfo>();
        private readonly Collection<DateTime> _selectedDates = new Collection<DateTime>();

        private bool _modifierKeyIsPressed;
        private Color _selectionColor;

        public CustomScheduleGrid()
        {
            BoostMaxConflicts();
            SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
            ThemesEnabled = true;
        }

        private void BoostMaxConflicts()
        {
            var maxConflictsField = typeof(ScheduleGrid).GetField("maxConflicts",
                                                                  BindingFlags.Instance | BindingFlags.NonPublic);
            maxConflictsField.SetValue(this, 16);
        }

        /// <summary>
        /// Gets a value indicating whether [CTRL pressing].
        /// </summary>
        /// <value><c>true</c> if [CTRL pressing]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-10
        /// </remarks>
        public bool ModifierKeyIsPressed
        {
            get { return _modifierKeyIsPressed; }
        }

        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <value>The dates.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-10
        /// </remarks>
        public IList<DateTime> Dates
        {
            get { return _selectedDates; }
        }

        /// <summary>
        /// Occurs when [appointments copy].
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public event EventHandler<EventArgs> AppointmentsCopy;

        /// <summary>
        /// Occurs when [appointments paste].
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        public event EventHandler<EventArgs> AppointmentsPaste;

        protected override void OnDrawCellDisplayText(GridDrawCellDisplayTextEventArgs e)
        {
            //handle the monthview
            if (ScheduleType == ScheduleViewType.Month)
            {
                DrawMonthView(e);
            }
            else
            {
                DrawWeekView(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);
            }
            catch (NullReferenceException)
            {
            }
        }

        protected override void OnQueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (e.Style.CellIdentity != null)
            {
                base.OnQueryCellInfo(e);
                int col = e.Style.CellIdentity.ColIndex;
                int row = e.Style.CellIdentity.RowIndex;
                if (!MarkCol(col) && col > MarkColumn && row > AllDayRow && (ScheduleType == ScheduleViewType.Day || ScheduleType == ScheduleViewType.WorkWeek || ScheduleType == ScheduleViewType.CustomWeek))
                {
                    FieldInfo fi = typeof(ScheduleGrid).GetField("ScheduleAppointmentFromRange", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
                    Hashtable ht = fi.GetValue(this) as Hashtable;
                    int key = GetLookupKey(row, col);
                    if (ht.ContainsKey(key))
                    {
                        e.Style.BackColor = Schedule.Appearance.PrimeTimeCellColor;
                        e.Style.Borders.Top = GridBorder.Empty;// new GridBorder(GridBorderStyle.Solid, Color.Gray, GridBorderWeight.Thin);//occupiedBorder;
                        e.Style.BorderMargins.Top = 0;
                        e.Style.Borders.Bottom = (row % 2) == 1 ? new GridBorder(GridBorderStyle.Solid, Schedule.Appearance.SolidBorderColor, GridBorderWeight.Thin)
                            : new GridBorder(Model.Options.DefaultGridBorderStyle);
                        e.Style.BorderMargins.Bottom = 0;
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.Control && !e.Shift)
            {
                _modifierKeyIsPressed = false;
            }

            base.OnKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _modifierKeyIsPressed = false;
            if (e.Control || e.Shift)
                _modifierKeyIsPressed = true;

            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Workaround for SPI 10215.
            // For some reason the grid looses the SelectCellsMouseButtonsMask setting all the time
            // /Henry
            SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            base.OnMouseDown(e);
        }

        protected override void OnCellClick(GridCellClickEventArgs e)
        {
            GridStyleInfo cellStyle = Model[e.RowIndex, e.ColIndex];
            if (cellStyle.CellType == "AllDayGrid" || Schedule.ScheduleType == ScheduleViewType.Month || Schedule.ScheduleType == ScheduleViewType.Day)
            {
                if (ModifierKeyIsPressed)
                {
                    bool removed = DeSelectDay(cellStyle);
                    if (!removed)
                        SelectAllDay(cellStyle, _selectionColor);
                }
                else
                {
                    ResetAllDayColor();
                    SelectAllDay(cellStyle, _selectionColor);
                }
            }
            else
            {
                ResetAllDayColor();
            }
            base.OnCellClick(e);
        }

        private bool DeSelectDay(GridStyleInfo cellStyle)
        {
            bool removed = false;
            for (int i = 0; i < _selectedCellStyleList.Count; i++)
            {
                if (Schedule.ScheduleType == ScheduleViewType.CustomWeek && _selectedCellStyleList[i].CellIdentity.ColIndex == cellStyle.CellIdentity.ColIndex)
                {
                    _selectedCellStyleList.RemoveAt(i);
                    cellStyle.BackColor = _allDayColor;
                    removed = true;
                    break;
                }
                if (Schedule.ScheduleType == ScheduleViewType.Month &&
                    _selectedCellStyleList[i].CellIdentity.ColIndex == cellStyle.CellIdentity.ColIndex &&
                    _selectedCellStyleList[i].CellIdentity.RowIndex == cellStyle.CellIdentity.RowIndex)
                {
                    _selectedCellStyleList.RemoveAt(i);
                    cellStyle.BackColor = _allDayColor;
                    removed = true;
                    break;
                }
            }
            return removed;
        }

        private void ResetAllDayColor()
        {
            foreach (GridStyleInfo style in _selectedCellStyleList)
            {
                style.BackColor = _allDayColor;
            }
            _selectedCellStyleList.Clear();
            _selectedDates.Clear();
        }
        private void SelectAllDay(GridStyleInfo style, Color color)
        {
            _selectedCellStyleList.Add(style);
            style.BackColor = color;
        }

        public void AddSelectedDate(DateTime date)
        {
            if (_modifierKeyIsPressed)
            {
                if (!Dates.Contains(date))
                    Dates.Add(date);
                else
                    Dates.Remove(date);
            }
            else
            {
                Dates.Clear();
                Dates.Add(date);
            }
        }

        public override string GetFormattedString(DateTime dt, string format)
        {
            return dt.ToString(format, Schedule.Culture);
        }

        /// <summary>
        /// Draws the round rect special.
        /// </summary>
        /// <param name="pointX">The point X.</param>
        /// <param name="pointY">The point Y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        public static GraphicsPath DrawRoundRectSpecial(float pointX, float pointY, float width, float height, float radius)
        {
            //Add padd from Right
            width -= 10;

            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(pointX + radius, pointY, pointX + width - (radius * 2), pointY);
            gp.AddArc(pointX + width - (radius * 2), pointY, radius * 2, radius * 2, 270, 90);
            gp.AddLine(pointX + width, pointY + radius, pointX + width, pointY + height - (radius * 2));
            gp.AddArc(pointX + width - (radius * 2), pointY + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            gp.AddLine(pointX + width - (radius * 2), pointY + height, pointX + radius, pointY + height);
            gp.AddArc(pointX, pointY + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddLine(pointX, pointY + height - (radius * 2), pointX, pointY + radius);
            gp.AddArc(pointX, pointY, radius * 2, radius * 2, 180, 90);
            gp.CloseFigure();

            return gp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomScheduleGrid"/> class.
        /// </summary>
        /// <param name="calendar">The <see cref="P:Syncfusion.Windows.Forms.Schedule.ScheduleGrid.Calendar"/> used to determine the dates displayed.</param>
        /// <param name="schedule">Gets the <see cref="T:Syncfusion.Windows.Forms.Schedule.ScheduleControl"/> that hosts this ScheduleGrid.</param>
        /// <param name="theDate">The date used to set the initial display.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        public CustomScheduleGrid(NavigationCalendar calendar, ScheduleControl schedule, DateTime theDate)
            : base(calendar, schedule, theDate)
        {
            //for copy/paste of schedulecontrol
            ClipboardCopy += CustomScheduleGrid_ClipboardCopy;
            ClipboardPaste += CustomScheduleGrid_ClipboardPaste;
            SelectionChanging += (CustomScheduleGrid_SelectionChanging);
           

            AllDayColor();
            BoostMaxConflicts();
            SelectCellsMouseButtonsMask = MouseButtons.Left | MouseButtons.Right;
            ThemesEnabled = true;
        }

        void CustomScheduleGrid_SelectionChanging(object sender, GridSelectionChangingEventArgs e)
        {
            if (MouseButtons == MouseButtons.Right)
            {
                //Code for making the selections to work as in Outlook
                foreach (GridRangeInfo selection in Selections.Ranges)
                {
                    if (selection.Contains(e.ClickRange))
                        e.Cancel = true;
                }
            }
        }

        private void AllDayColor()
        {
            _allDayColor = Color.White;
            _selectionColor = Color.WhiteSmoke;
        }

        /// <summary>
        /// Handles the ClipboardPaste event of the CustomScheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCutPasteEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        private void CustomScheduleGrid_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            OnAppointmentsPaste();
        }

        /// <summary>
        /// Handles the ClipboardCopy event of the CustomScheduleGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCutPasteEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 11/26/2008
        /// </remarks>
        private void CustomScheduleGrid_ClipboardCopy(object sender, GridCutPasteEventArgs e)
        {
            OnAppointmentsCopy();
            _modifierKeyIsPressed = false;
        }

        protected void OnAppointmentsCopy()
        {
            if (AppointmentsCopy != null)
            {
                AppointmentsCopy(this, new EventArgs());
            }
        }

        protected void OnAppointmentsPaste()
        {
            if (AppointmentsPaste != null)
            {
                AppointmentsPaste(this, new EventArgs());
            }
        }

        private bool MarkCol(int col)
        {
            return (col % (DayColCount - 1)) == MarkColumn;
        }

        /// <summary>
        /// Gets the lookup key.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="colIndex">Index of the col.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        private static int GetLookupKey(int rowIndex, int colIndex)
        {
            return 10000 * rowIndex + colIndex;
        }

        /// <summary>
        /// Draws the month view.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellDisplayTextEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        private void DrawMonthView(GridDrawCellDisplayTextEventArgs e)
        {
            //if (DateTime.Now.Millisecond < 10)
            //{
            //    return;
            //}
            ///TODO : needto refactor when syncfusion Volume 4 is release
            string[] list = e.DisplayText.Split(new[] { '\n' });
            int n = list.GetLength(0) - 1;

            if (n > 0)
            {
                Rectangle rect = e.TextRectangle;
                int h = (int)(e.Style.Font.Size + 1);
                rect.Height = 2 * h - 1;

                //bool isFirstCell = e.ColIndex == 1 && e.RowIndex == 1;
                //DateTime dt = isFirstCell
                //                  ? Calendar.SelectedDates[0]
                //                  : DateTime.Parse(list[0], Schedule.Culture).Date;
                DateTime dt = DateTime.Parse(list[0], Schedule.Culture).Date;

                //draw the header
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
                e.Style.VerticalAlignment = GridVerticalAlignment.Top;
                e.Style.WrapText = false;
                string s;
                var dayOfMonth = Schedule.Culture.Calendar.GetDayOfMonth(dt);
                if (dayOfMonth == 1)
                {
                    //long date pattern
                    s = (Schedule.Culture.Calendar.GetMonth(dt) == 1)
                            ? dt.ToString(DATEFORMAT_WEEK_MONTH_NEWYEAR, Schedule.Culture)
                            : dt.ToString(DATEFORMAT_WEEK_MONTH_NEWMONTH, Schedule.Culture);
                }
                else
                {
                    s = (e.Style.CellIdentity.RowIndex == 1 && e.Style.CellIdentity.ColIndex == 1)
                            ? dt.ToString(DATEFORMAT_WEEK_MONTH_NEWMONTH, Schedule.Culture)
                            : dayOfMonth.ToString(Schedule.Culture);
                }

                if (CurrentCell.HasCurrentCellAt(e.Style.CellIdentity.RowIndex, e.Style.CellIdentity.ColIndex))
                {
                    // draw back ground
                    Rectangle rect1 = e.TextRectangle;
                    rect1.Height = 2 * h - 1;
                    switch (Schedule.Appearance.VisualStyle)
                    {
                        case GridVisualStyles.SystemTheme:
                            using (Brush b = new LinearGradientBrush(rect, Schedule.Appearance.MonthWeekHeaderForeColor, Schedule.Appearance.MonthWeekHeaderBackColor,
                                                            LinearGradientMode.Vertical))
                            {
                                e.Graphics.FillRectangle(b, rect1);
                            }
                            break;

                        default:
                            Model.Options.GridVisualStylesDrawing.DrawHeaderStyle(e.Graphics, rect1, ThemedHeaderDrawing.HeaderState.Pressed);
                            break;
                    }
                }

                GridStaticCellRenderer.DrawText(e.Graphics, s, e.Style.GdipFont, rect, e.Style, e.Style.TextColor, false);
                DrawShiftInformation(e, dt);

                e.Cancel = true;
            }

        }

        /// <summary>
        /// Draws the week view.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellDisplayTextEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-16
        /// </remarks>
        private void DrawWeekView(GridDrawCellDisplayTextEventArgs e)
        {
            int col = e.Style.CellIdentity.ColIndex;
            int row = e.Style.CellIdentity.RowIndex;

            FieldInfo fi = typeof(ScheduleGrid).GetField("ScheduleAppointmentFromRange", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
            Hashtable ht = fi.GetValue(this) as Hashtable;
            int key = GetLookupKey(row, col);
            if (ht != null && ht.ContainsKey(key))
            {
                ICustomScheduleAppointment scheduleItem = ht[key] as ICustomScheduleAppointment;

                Color c = ((ListObject)Schedule.DataSource.GetLabels()[scheduleItem.LabelValue]).ColorMember;
                string text = ParseDisplayItem(ht[key] as IScheduleAppointment, Schedule.Appearance.DayItemFormat);

                if (scheduleItem.Tag is PersonRequestDto)
                {
                    DrawRequest(e, scheduleItem.Tag, c, text);
                }
                else if (scheduleItem.Tag is PersonMeetingDto)
                {
                    DrawPersonMeeting(e, scheduleItem.Tag, c, text);
                }
                else if (scheduleItem.Tag is PersonalShiftActivityDto)
                {
                    DrawPersonalShift(e, scheduleItem.Tag, c, text);
                }
                else
                {
                    //Having a rectangle with width 1 creates exceptions in further drawing. Set to 0 fixes the issue.
                    if (e.TextRectangle.Width == 1)
                        e.TextRectangle = new Rectangle(e.TextRectangle.X, e.TextRectangle.Y, 0, e.TextRectangle.Height);
                    base.OnDrawCellDisplayText(e);
                }
            }
            else
            {
                base.OnDrawCellDisplayText(e);
            }
        }

        /// <summary>
        /// Draws the shift information.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellDisplayTextEventArgs"/> instance containing the event data.</param>
        /// <param name="shiftDate">The shift date.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-12
        /// </remarks>
        private static void DrawShiftInformation(GridDrawCellDisplayTextEventArgs e, DateTime shiftDate)
        {
            // ShiftInformationDto shiftInfor = CustomScheduleDataProvider.GetShift(shiftDate);

            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = shiftDate;
            IDictionary<DateTime, SchedulePartDto> schedulePartDictionary = AgentScheduleStateHolder.Instance().AgentSchedulePartDictionary;
            if (!schedulePartDictionary.ContainsKey(shiftDate))
            {
                return;
            }
            SchedulePartDto schedulePartDto =
                AgentScheduleStateHolder.Instance().AgentSchedulePartDictionary[shiftDate];

            if (schedulePartDto != null)
            {
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
                e.Style.WrapText = false;
                e.Style.Font.Bold = true;

                Rectangle shiftRect = e.TextRectangle;
                shiftRect.Y -= 20;

                StringBuilder drawString = new StringBuilder();


                if (schedulePartDto.PersonDayOff != null && schedulePartDto.ProjectedLayerCollection.Count == 0)
                    drawString.AppendLine(schedulePartDto.PersonDayOff.Name);
                else
                {
                    if (schedulePartDto.IsFullDayAbsence &&
                        schedulePartDto.ProjectedLayerCollection.Count > 0 &&
                            schedulePartDto.ProjectedLayerCollection.First().IsAbsence)
                    {
                        drawString.AppendLine(schedulePartDto.ProjectedLayerCollection.First().Description);
                        drawString.AppendLine();
                    }
                    else
                    {
                        if (schedulePartDto.PersonAssignmentCollection.Count > 0 && schedulePartDto.PersonAssignmentCollection.Last().MainShift != null)
                        {
                            string categoryName =
                                schedulePartDto.PersonAssignmentCollection.Last().MainShift.ShiftCategoryName;

                            drawString.AppendLine(categoryName);
                        }
                        if (schedulePartDto.ProjectedLayerCollection.Count > 0)
                        {
                            drawString.AppendLine(schedulePartDto.ProjectedLayerCollection.First().Period.LocalStartDateTime.ToShortTimeString() + " - " +
                                schedulePartDto.ProjectedLayerCollection.Last().Period.LocalEndDateTime.ToShortTimeString());

                            DateTime contractTime = schedulePartDto.ContractTime;
                            drawString.AppendLine(contractTime.ToString("HH:mm", CultureInfo.CurrentCulture));
                        }
                    }
                }
                GridStaticCellRenderer.DrawText(e.Graphics, drawString.ToString(), e.Style.GdipFont, shiftRect, e.Style,
                                               e.Style.TextColor, false);
            }
        }

        /// <summary>
        /// Draws the requests.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellDisplayTextEventArgs"/> instance containing the event data.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="displayText">The display text.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-12
        /// </remarks>
        private static void DrawRequest(GridDrawCellDisplayTextEventArgs e, Object tag, Color c, string displayText)
        {
            PersonRequestDto personRequest = tag as PersonRequestDto;

            if (personRequest != null)
            {
                Rectangle drawingRect = e.TextRectangle;
                drawingRect.Inflate(-3, 0);
                GraphicsPath myGraphicsPath = DrawRoundRectSpecial(drawingRect.X, drawingRect.Y,
                    drawingRect.Width, drawingRect.Height, _roundedCornersRadius); //4

                // If request use pattern to visualize requests
                using (HatchBrush brush = new HatchBrush(_requestHatchStyle, c, Color.Transparent))
                {
                    e.Graphics.FillPath(brush, myGraphicsPath);
                }

                //draw Icon
                Rectangle rect2 = new Rectangle(drawingRect.Right - 30, drawingRect.Bottom - 16, 16, 16);
                if (personRequest.RequestStatus == RequestStatusDto.Approved)
                {
                    e.Graphics.DrawIconUnstretched(Resources.ccc_Approve, rect2);
                }
                else
                {
                    e.Graphics.DrawIconUnstretched(Resources.ccc_Denied, rect2);
                }

                using (Pen pen = new Pen(Color.Black))
                {
                    pen.Brush = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.Black);

                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.DrawPath(pen, myGraphicsPath);
                }

                myGraphicsPath.Dispose();

                drawingRect.Offset(drawingRect.Height / 16, 2);   //rect.Height / 4
                drawingRect.Width -= drawingRect.Height / 16 * 2; //rect.Height / 4
                drawingRect.Height -= 2 * 2;

                GridStaticCellRenderer.DrawText(e.Graphics, displayText, e.Style.GdipFont, drawingRect,
                                                e.Style, e.Style.TextColor, e.Style.RightToLeft == RightToLeft.Yes);
                e.Cancel = true;
            }
        }
        
        /// <summary>
        /// Draws the person meeting.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellDisplayTextEventArgs"/> instance containing the event data.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="c">The c.</param>
        /// <param name="displayText">The display text.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2/13/2009
        /// </remarks>
        private static void DrawPersonMeeting(GridDrawCellDisplayTextEventArgs e, Object tag, Color c, string displayText)
        {
            PersonMeetingDto personMeetingDto = tag as PersonMeetingDto;

            if (personMeetingDto != null)
            {
                Rectangle drawingRect = e.TextRectangle;
                drawingRect.Inflate(-3, 0);

                using (GraphicsPath myGraphicsPath = DrawRoundRectSpecial(drawingRect.X, drawingRect.Y,
                   drawingRect.Width, drawingRect.Height, _roundedCornersRadius))
                {
                    // If request use pattern to visualize requests
                    using (HatchBrush brush = new HatchBrush(_personMeetingHatchStyle, c, Color.Transparent))
                    {
                        e.Graphics.FillPath(brush, myGraphicsPath);
                    }

                    using (Pen pen = new Pen(Color.Black))
                    {
                        pen.Brush = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.Black);

                        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                        e.Graphics.DrawPath(pen, myGraphicsPath);
                    }
                }

                drawingRect.Offset(drawingRect.Height / 16, 2);   //rect.Height / 4
                drawingRect.Width -= drawingRect.Height / 16 * 2; //rect.Height / 4
                drawingRect.Height -= 2 * 2;
                
                GridStaticCellRenderer.DrawText(e.Graphics, displayText, e.Style.GdipFont, drawingRect,
                                                e.Style, e.Style.TextColor, e.Style.RightToLeft == RightToLeft.Yes);
                e.Cancel = true;
            }
        }


        private static void DrawPersonalShift(GridDrawCellDisplayTextEventArgs e, Object tag, Color c, string displayText)
        {
            PersonalShiftActivityDto personalShiftActivityDto = tag as PersonalShiftActivityDto;

            if (personalShiftActivityDto != null)
            {
                Rectangle drawingRect = e.TextRectangle;
                drawingRect.Inflate(-3, 0);

                using (GraphicsPath myGraphicsPath = DrawRoundRectSpecial(drawingRect.X, drawingRect.Y,
                   drawingRect.Width, drawingRect.Height, _roundedCornersRadius))
                {
                    // If request use pattern to visualize requests
                    using (HatchBrush brush = new HatchBrush(_personalShiftHatchStyle, c, Color.Transparent))
                    {
                        e.Graphics.FillPath(brush, myGraphicsPath);
                    }

                    using (Pen pen = new Pen(Color.Black))
                    {
                        pen.Brush = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.Black);

                        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                        e.Graphics.DrawPath(pen, myGraphicsPath);
                    }
                }

                drawingRect.Offset(drawingRect.Height / 16, 2);   //rect.Height / 4
                drawingRect.Width -= drawingRect.Height / 16 * 2; //rect.Height / 4
                drawingRect.Height -= 2 * 2;

                GridStaticCellRenderer.DrawText(e.Graphics, displayText, e.Style.GdipFont, drawingRect,
                                                e.Style, e.Style.TextColor, e.Style.RightToLeft == RightToLeft.Yes);
                e.Cancel = true;
            }
        }

    }
}