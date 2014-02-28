using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
    /// <summary>
    /// Base view class
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract class ScheduleViewBase : AddScheduleLayers, IScheduleViewBase, IDisposable, IHelpContext
    {
        private readonly GridControl _grid;
        private Font _cellFontBig;
        private Font _cellFontSmall;
        private Font _fontTimeLine;
        private Color _colorHolidayCell;
        private Color _colorHolidayCellNotPublished;
        private Color _colorCellNotPublished;
        private Color _colorHolidayHeader;
        private readonly IHandleBusinessRuleResponse _handleBusinessRuleResponse = new HandleBusinessRuleResponse();
        private bool _isDesposing;
		private bool _isOverviewColumnsHidden;
        private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();
        private delegate void RefreshRangeForAgentPeriodHandler(IEntity person, DateTimePeriod period);

        public event EventHandler RefreshSelectionInfo;
        public event EventHandler RefreshShiftEditor;
        public event EventHandler<EventArgs> ViewPasteCompleted;

        protected ScheduleViewBase(GridControl grid)
            : base(null)
        {
            setColors();
            _grid = grid;
            grid.HideCols.ResetRange(0, 300);
        }

		public void Sort(IScheduleSortCommand command)
		{
			var selectedSchedulePart = SelectedSchedules().FirstOrDefault();
			if (selectedSchedulePart == null) return;
			Presenter.SortCommand = command;
			Presenter.SortCommand.Execute(selectedSchedulePart.DateOnlyAsPeriod.DateOnly);
			SetSelectionFromParts(new List<IScheduleDay> { selectedSchedulePart });
			ViewGrid.Refresh();
		}
    	
    	public bool IsOverviewColumnsHidden	
    	{
    		get
    		{
				if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
					return true;

    			return _isOverviewColumnsHidden;
    		}
    		set { _isOverviewColumnsHidden = value; }
    	}

    	protected override void OnPresenterSet()
        {
            base.OnPresenterSet();
            setEventHandlers(true);
        }
        

        private void setColors()
        {
            _cellFontBig = ColorHelper.ScheduleViewBaseCellFontBig;
            _cellFontSmall = ColorHelper.ScheduleViewBaseCellFontSmall;
            _fontTimeLine = ColorHelper.ScheduleViewBaseCellFontTimeline;
            _colorHolidayCell = ColorHelper.ScheduleViewBaseHolidayCell;
            _colorHolidayCellNotPublished = ColorHelper.ScheduleViewBaseHolidayCellNotPublished;
            _colorCellNotPublished = ColorHelper.ScheduleViewBaseCellNotPublished;
            _colorHolidayHeader = ColorHelper.ScheduleViewBaseHolidayHeader;
        }

        private void setEventHandlers(bool isAdd)
        {
            if (_grid == null || Presenter==null) return;

            if (isAdd)
                _grid.QueryCellInfo += Presenter.QueryCellInfo;
            else
                _grid.QueryCellInfo -= Presenter.QueryCellInfo;

            if (isAdd)
                _grid.QueryColCount += QueryColCount;
            else
                _grid.QueryColCount -= QueryColCount;
            
            if (isAdd)
                _grid.QueryRowCount += QueryRowCount;
            else
                _grid.QueryRowCount -= QueryRowCount;
            
            if (isAdd)
                _grid.SaveCellInfo += SaveCellInfo;
            else
                _grid.SaveCellInfo -= SaveCellInfo;
            
            if (isAdd)
                _grid.CellDrawn += CellDrawn;
            else
                _grid.CellDrawn -= CellDrawn;

            if (isAdd)
                _grid.QueryColWidth += QueryColWidth;
            else
                _grid.QueryColWidth -= QueryColWidth;
            
            if (isAdd)
                _grid.QueryRowHeight += QueryRowHeight;
            else
                _grid.QueryRowHeight -= QueryRowHeight;
            
            if (isAdd)
                _grid.CellClick += CellClick;
            else
                _grid.CellClick -= CellClick;
            
            if (isAdd)
                _grid.CellDoubleClick += CellDoubleClick;
            else
                _grid.CellDoubleClick -= CellDoubleClick;
            
            if (isAdd)
                _grid.MouseDown += mouseDown;
            else
                _grid.MouseDown -= mouseDown;
            
            if (isAdd)
                _grid.MouseUp += MouseUp;
            else
                _grid.MouseUp -= MouseUp;

			if (isAdd)
				_grid.CurrentCellMoving += GridCurrentCellMoving;
			else
				_grid.CurrentCellMoving -= GridCurrentCellMoving;
        }

		internal virtual void GridCurrentCellMoving(object sender, GridCurrentCellMovingEventArgs e)
		{
			
		}


        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                GridRangeInfo rangeInfo = _grid.PointToRangeInfo(e.Location);
                if (!_grid.Selections.Ranges.AnyRangeIntersects(rangeInfo))
                {
                    _grid.Selections.Clear(true);
                    _grid.CurrentCell.MoveTo(rangeInfo, GridSetCurrentCellOptions.None);
                }
            }
        }

        private void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            e.Handled = true;
        }

        public void LoadScheduleViewGrid()
        {
            _grid.BeginUpdate();

            CreateHeaders();
            CreateCellModels();

            Presenter.MergeHeaders();
            ViewGrid.Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());

            _grid.EndUpdate();

            _grid.Refresh();

            _grid.Model.ScrollCellInView(GridRangeInfo.Col(_grid.CurrentCell.ColIndex), GridScrollCurrentCellReason.MoveTo);
            if (IsOverviewColumnsHidden)
            {
                GridRangeInfo nameColumnRangeInfo = GridRangeInfo.Col((int)ColumnType.RowHeaderColumn);
                _grid.Model.ColWidths.ResizeToFit(nameColumnRangeInfo, GridResizeToFitOptions.IncludeCellsWithinCoveredRange);
            }
            else
            {
                _grid.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols((int)ColumnType.RowHeaderColumn, (int)ColumnType.StartScheduleColumns - 1));
            }
        }

        /// <summary>
        /// Get big cell font
        /// </summary>
        public Font CellFontBig
        {
            get { return _cellFontBig; }
        }

        /// <summary>
        /// get small cell font
        /// </summary>
        public Font CellFontSmall
        {
            get { return _cellFontSmall; }
        }

        /// <summary>
        /// get timeline font
        /// </summary>
        public Font TimelineFont
        {
            get { return _fontTimeLine; }
        }

        /// <summary>
        /// Get grid
        /// </summary>
        public GridControl ViewGrid
        {
            get { return _grid; }
        }

        /// <summary>
        /// CellWidth
        /// </summary>
        protected virtual int CellWidth()
        {
            return 0;
        }

        /// <summary>
        /// Number of rowheaders
        /// </summary>
        public int RowHeaders
        {
            get { return ViewGrid.Rows.HeaderCount; }
        }

        /// <summary>
        /// Number of colheaders
        /// </summary>
        public int ColHeaders
        {
            get { return ViewGrid.Cols.HeaderCount; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ViewBase"/> is RigthToLeft.
        /// </summary>
        /// <value><c>true</c> if RigthToLeft; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-30
        /// </remarks>
        public bool IsRightToLeft
        {
            get
            {
                return (_grid.RightToLeft == RightToLeft.Yes);
            }
            set {
                _grid.RightToLeft = value ? RightToLeft.Yes : RightToLeft.No;
            }
        }

        public virtual bool PartIsEditable()
        {
            return true;
        }
        private void sortColumn(int column)
        {
            // Cancel edit
            if (ViewGrid.CurrentCell.IsEditing)
            {
                ViewGrid.CurrentCell.EndEdit();
            }

            Presenter.SortColumn(column);

            // Repopulate the grid
            ViewGrid.Refresh();
        }

        /// <summary>
        /// CellDoubleClick
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellClickEventArgs"/> instance containing the event data.</param>
        internal void CellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            // Sort on left double click
            if (e.RowIndex == 1 &&
                e.ColIndex < (int)ColumnType.StartScheduleColumns &&
                e.MouseEventArgs.Button == MouseButtons.Left)
            {
                sortColumn(e.ColIndex);
            }
        }

        /// <summary>
        /// CellClick
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellClickEventArgs"/> instance containing the event data.</param>
        internal virtual void CellClick(object sender, GridCellClickEventArgs e)
        {
        }

        public void AddWholeWeekAsSelected(int rowIndex, int colIndex)
        {
            if (_grid.Model.ColCount < colIndex) return;
            _grid.Model.Selections.Clear(true);
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
            int weekNumWeekHeader = DateHelper.WeekNumber(((DateOnly)_grid.Model[rowIndex, colIndex].Tag).Date, culture);
            for (int i = (int)ColumnType.StartScheduleColumns; i <= _grid.ColCount; i++)
            {
                int weekNumDateHeader = DateHelper.WeekNumber(((DateOnly)_grid.Model[rowIndex + 1, i].Tag).Date, culture);
                if (weekNumWeekHeader == weekNumDateHeader)
                {
                    _grid.Model.Selections.Add(GridRangeInfo.Cols(i, i));

                    _grid.CurrentCell.MoveTo(_grid.Model.SelectedRanges[0].Top,
                                             _grid.Model.SelectedRanges[0].Left);
                }
            }
            var start = _grid.Model.Selections.Ranges[0].Left;
            var end = _grid.Model.Selections.Ranges[_grid.Model.Selections.Ranges.Count - 1].Right;
            _grid.Model.Selections.Clear(true);
            _grid.Model.Selections.Add(GridRangeInfo.Cols(start, end));
        }

        internal virtual void MouseUp(object sender, MouseEventArgs e)
        {
            GridRangeInfo range = _grid.PointToRangeInfo(e.Location);

            if (range.Top > 1 && range.Left == 1)
            {
                GridRangeInfoList list = GridHelper.GetGridSelectedRanges(_grid, false);
                _grid.Model.Selections.Clear(true);

                foreach (GridRangeInfo rangeInfo in list)
                {
                    if (rangeInfo.Left == 0 && rangeInfo.Top > 0)
                    {
                        _grid.Model.Selections.Add(GridRangeInfo.Cells(rangeInfo.Top, (int)ColumnType.StartScheduleColumns, rangeInfo.Bottom, _grid.ColCount));
                        _grid.CurrentCell.MoveTo(_grid.Model.SelectedRanges[0].Top,
                                                 _grid.Model.SelectedRanges[0].Left);
                    }
                }
            }
        }

        /// <summary>
        /// Col width query for grid
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColSizeEventArgs"/> instance containing the event data.</param>
        internal virtual void QueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index == (int)ColumnType.None)
            {
                // Hide
                e.Size = 0;
                e.Handled = true;
            }
            else if (e.Index == (int)ColumnType.RowHeaderColumn)
            {
                // Auto size
            }
            else if (e.Index < (int)ColumnType.StartScheduleColumns)
            {
                if (IsOverviewColumnsHidden)
                {
                    // Hide
                    e.Size = 0;
                    e.Handled = true;
                }
            }
            else
            {
                e.Size = CellWidth();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Row height query for grid
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColSizeEventArgs"/> instance containing the event data.</param>
        internal virtual void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
        }

        /// <summary>
        /// Col count query for grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal virtual void QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = Presenter.ColCount;
            e.Handled = true;
        }

        /// <summary>
        /// Row count query for grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal virtual void QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            if(!_isDesposing)
                e.Count = Presenter.RowCount;
            e.Handled = true;
        }

        /// <summary>
        /// Use this default implementation to get the correct background if e.Cancel
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        internal virtual void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
        }

        /// <summary>
        /// CreateProjection headers
        /// </summary>
        internal virtual void CreateHeaders()
        {
            ViewGrid.Rows.HeaderCount = 1; //2 row headers, week and date
            ViewGrid.Cols.HeaderCount = 1;

            // Reset merge if grid is reused
            ViewGrid.Model.Options.MergeCellsMode = GridMergeCellsMode.None;
            // Set merge
            ViewGrid.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation |
                                                    GridMergeCellsMode.MergeColumnsInRow;
            ViewGrid.Rows.FrozenCount = 1;
            ViewGrid.Cols.FrozenCount = (int)ColumnType.StartScheduleColumns -1;
        }

        /// <summary>
        /// Create cell models to format CellValue
        /// </summary>
        internal void CreateCellModels()
        {
            if (!ViewGrid.CellModels.ContainsKey("TotalDayOffCell"))
                ViewGrid.CellModels.Add("TotalDayOffCell", new NumericReadOnlyCellModel(ViewGrid.Model){NumberOfDecimals = 0});
            if (!ViewGrid.CellModels.ContainsKey("TotalTimeCell"))
                ViewGrid.CellModels.Add("TotalTimeCell", new TimeSpanDurationStaticCellModel(ViewGrid.Model));
            if (!ViewGrid.CellModels.ContainsKey("RestrictionWeekHeaderViewCellModel"))
                ViewGrid.Model.CellModels.Add("RestrictionWeekHeaderViewCellModel", new RestrictionWeekHeaderViewCellModel(ViewGrid.Model));
        }

        //return tip for a day header
        public virtual string DayHeaderTooltipText(GridStyleInfo gridStyle,DateOnly currentDate)
        {
            return gridStyle.CellTipText = string.Concat(CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(CultureInfo.CurrentUICulture.Calendar.GetDayOfWeek(currentDate)), " ",
               currentDate.ToShortDateString(CultureInfo.CurrentCulture));
        }

        protected void DrawSchedule(GridDrawCellEventArgs e, DateDateTimePeriodDictionary timeSpans, SchedulePartView significantPart)
        {
            IScheduleDay scheduleRange = e.Style.CellValue as IScheduleDay;
            if (scheduleRange != null)
            {
                IVisualLayerCollection layerCollection = scheduleRange.ProjectionService().CreateProjection();
                foreach (IVisualLayer layer in layerCollection)
                {
					DrawLayer(e, layer, timeSpans, scheduleRange.Person, significantPart);
                }

                if (!layerCollection.Any())
                {
					if(significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
					{
						var absenceCollection = scheduleRange.PersonAbsenceCollection();
						IPersonAbsence personAbsence = absenceCollection[absenceCollection.Count - 1];
						IVisualLayerFactory layerFactory = new VisualLayerFactory();
						IVisualLayer actLayer = layerFactory.CreateShiftSetupLayer(new Activity("activity"), personAbsence.Period, personAbsence.Person);
	
						DrawLayer(e, layerFactory.CreateAbsenceSetupLayer(personAbsence.Layer.Payload, actLayer, personAbsence.Period), timeSpans, scheduleRange.Person, significantPart);
					}
                }
            }
        }

        public void SetCellBackTextAndBackColor(GridQueryCellInfoEventArgs e, DateTime dateTime, bool backColor, bool textColor, IScheduleDay schedulePart)
        {
            Color backGroundColor = _grid.BackColor;
            Color backGroundHolidayColor = _colorHolidayCell;

            var authorization = PrincipalAuthorization.Instance();
            if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
            {
                if (schedulePart != null)
                {
                    //check if schedule is published
                    if (!schedulePart.IsFullyPublished)
                    {
                        //add not publisched colors
                        backGroundColor = _colorCellNotPublished;
                        backGroundHolidayColor = _colorHolidayCellNotPublished;
                    }
                }
            }
            
                //apply colors
            if (DateHelper.IsWeekend(dateTime, CultureInfo.CurrentCulture))
            {
                if (backColor) e.Style.BackColor = backGroundHolidayColor;
                if (textColor) e.Style.TextColor = _colorHolidayHeader;
            }
            else
            {
                if(backColor)
                    e.Style.BackColor = backGroundColor;

                e.Style.TextColor = _grid.ForeColor;

            }
        }

        private static void addAbsenceMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange, SchedulePartView significantPart)
        {
            if (significantPart != SchedulePartView.FullDayAbsence && significantPart != SchedulePartView.ContractDayOff)
            {
                var abs = (from l in scheduleRange.ProjectionService().CreateProjection()
                           where l.Payload is IAbsence
                           select l).ToList();

                if (!abs.IsEmpty() || significantPart == SchedulePartView.Absence)
                {
                    Point pt1 = new Point(e.Bounds.Left, e.Bounds.Bottom);
                    Point pt2 = new Point(e.Bounds.Left + 8, e.Bounds.Bottom);
                    Point pt3 = new Point(e.Bounds.Left, e.Bounds.Bottom - 8);

                    e.Graphics.FillPolygon(Brushes.Blue, new[] { pt1, pt2, pt3 });
                }
            }
        }

        private static void addConflictMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            //conflicts
            // Ola chenged from a check in conflicting ass to check on Businessrules
            if (scheduleRange.BusinessRuleResponseCollection.Count > 0 ) //&& _drawMarkers)
            {
                //check if we have conflicting  assignments
                Point pt1 = new Point(e.Bounds.X, e.Bounds.Y);
                Point pt2 = new Point(e.Bounds.X + 6, e.Bounds.Y);
                Point pt3 = new Point(e.Bounds.X, e.Bounds.Y + 6);

                e.Graphics.FillPolygon(Brushes.Red, new[] { pt1, pt2, pt3 });
            }
        }

        private static void addPersonalShiftMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            var personAssignment = scheduleRange.PersonAssignment();
					if (personAssignment != null)
					{
						if (personAssignment.PersonalActivities().Any())
						{
							Point pt1 = new Point(e.Bounds.Right, e.Bounds.Y);
							Point pt2 = new Point(e.Bounds.Right - 6, e.Bounds.Y);
							Point pt3 = new Point(e.Bounds.Right, e.Bounds.Y + 6);

							e.Graphics.FillPolygon(Brushes.Blue, new[] { pt1, pt2, pt3 });
						}
					}
        }

        private static void addOvertimeMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            var personAssignment = scheduleRange.PersonAssignment();
					if (personAssignment != null)
					{
						if (personAssignment.OvertimeActivities().Any())
						{
							Size s = new Size(6, 6);
							Point point1 = new Point(e.Bounds.Left, (e.Bounds.Y + e.Bounds.Height / 2) - 3);
							Rectangle rectangle = new Rectangle(point1, s);

							e.Graphics.FillRectangle(Brushes.Orange, rectangle);
						}
					}
        }

        private static void addMeetingMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            foreach (IPersonMeeting personMeeting in scheduleRange.PersonMeetingCollection())
            {
                if (personMeeting != null)
                {
                    Point pt1 = new Point(e.Bounds.Right, e.Bounds.Bottom);
                    Point pt2 = new Point(e.Bounds.Right - 8, e.Bounds.Bottom);
                    Point pt3 = new Point(e.Bounds.Right, e.Bounds.Bottom - 8);

                    e.Graphics.FillPolygon(Brushes.Black, new[] { pt1, pt2, pt3 });
                }
            }
        }

        private static void addNoteMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {

            bool noteExist = scheduleRange.NoteCollection().Any(n => n.GetScheduleNote(new NoFormatting()) != null);
            bool publicNoteExist = scheduleRange.PublicNoteCollection().Any(n => n.GetScheduleNote(new NoFormatting()) != null);

            if (noteExist || publicNoteExist)
            {
                Point pt1 = new Point(e.Bounds.Left, e.Bounds.Y);
                Point pt2 = new Point(e.Bounds.Left + 6, e.Bounds.Y);
                Point pt3 = new Point(e.Bounds.Left, e.Bounds.Y + 6);

                e.Graphics.FillPolygon(Brushes.Blue, new[] { pt1, pt2, pt3 });
            }
        }

        private void addLockMarkers(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            if (Presenter.LockManager.HasLocks)
            {
                GridlockDictionary gridLock = Presenter.LockManager.Gridlocks(scheduleRange);

                if (gridLock != null && gridLock.HasLockType(LockType.Normal))
                {
                    using (HatchBrush brush = new HatchBrush(HatchStyle.Percent20, Color.Gray, Color.Transparent))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
                if (gridLock != null && gridLock.HasLockType(LockType.Authorization))
                {
                    using (HatchBrush brush = new HatchBrush(HatchStyle.Percent05, Color.Gray, Color.White))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
                if (gridLock != null && gridLock.HasLockType(LockType.OutsidePersonPeriod))
                {
                    using (HatchBrush brush = new HatchBrush(HatchStyle.SmallGrid, Color.Gray, Color.Transparent))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
                if (gridLock != null && gridLock.HasLockType(LockType.WriteProtected))
                {
                    using (HatchBrush brush = new HatchBrush(HatchStyle.Percent10, Color.Red, Color.Transparent))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

            }
        }

        private void addFilterOnMeetingMarkers(GridDrawCellEventArgs e)
        {
            if (Presenter.SchedulePartFilter == SchedulePartFilter.Meetings)
            {
                using (HatchBrush brush = new HatchBrush(HatchStyle.Percent05, Color.LightGray, Color.Transparent))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }
        }
 
        protected void AddMarkersToCell(GridDrawCellEventArgs e, IScheduleDay scheduleRange, SchedulePartView significantPart)
        {  
            //conflicts
            addConflictMarkers(e, scheduleRange);

            //part day absences
            addAbsenceMarkers(e, scheduleRange, significantPart);

            //personal shifts
            addPersonalShiftMarkers(e, scheduleRange);

            //Overtime
            addOvertimeMarkers(e, scheduleRange);

            //meetings
            addMeetingMarkers(e, scheduleRange);

            //Notes
            addNoteMarkers(e, scheduleRange);

            //locks
            addLockMarkers(e, scheduleRange);

            //filter on meetings
            addFilterOnMeetingMarkers(e);
        }

        public static IAbsence SignificantAbsence(IScheduleDay scheduleRange)
        {
            IVisualLayerCollection layerCollection = scheduleRange.ProjectionService().CreateProjection();
            if (layerCollection.IsSatisfiedBy(VisualLayerCollectionSpecification.OneAbsenceLayer))
            {
                foreach (IVisualLayer layer in layerCollection)
                {
                    return layer.Payload as IAbsence;
                }
            }
            else
            {
                IAbsence ret = null;
                foreach (IPersonAbsence pa in scheduleRange.PersonAbsenceCollection())
                {
                    ret = pa.Layer.Payload;
                }

                return ret;
            }

            return null;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected void DrawLayer(GridDrawCellEventArgs e, ILayer<IPayload> layer, DateDateTimePeriodDictionary timeSpans, IPerson person, SchedulePartView significantPart)
        {
        	var dateOnly = (DateOnly) e.Style.Tag;
            DateTimePeriod period = timeSpans[dateOnly];
            Rectangle projectionRectangle = e.Bounds;

            if (e.Bounds.Height > SchedulePresenterBase.ProjectionHeight)
            {
                projectionRectangle = new Rectangle(e.Bounds.X, e.Bounds.Bottom - SchedulePresenterBase.ProjectionHeight, e.Bounds.Width, SchedulePresenterBase.ProjectionHeight);
            }

            Rectangle rect = ViewBaseHelper.GetLayerRectangle(period, projectionRectangle, layer.Period, IsRightToLeft);
            if (!rect.IsEmpty)
            {
				if (significantPart == SchedulePartView.ContractDayOff)
				{
					using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, layer.Payload.ConfidentialDisplayColor(person, dateOnly)))
					{
						e.Graphics.FillRectangle(brush, rect);
					}
				}
				else
				{
					using (LinearGradientBrush lBrush = GridHelper.GetGradientBrush(rect,layer.Payload.ConfidentialDisplayColor(person, dateOnly)))
					{
						e.Graphics.FillRectangle(lBrush, rect);
					}
				}
            }
        }

        internal void GridClipboardCopy(bool cut)
        {
            Presenter.ClipHandlerSchedule.IsInCutMode = cut;
            GridHelper.GridCopySelection(_grid, Presenter.ClipHandlerSchedule, true);
            _externalExceptionHandler.AttemptToUseExternalResource(()=>
            Clipboard.SetData("PersistableScheduleData", new int()));
        }

        //paste in views, only accepts paste from views
        public void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo)
        {
            if (Presenter.ClipHandlerSchedule.ClipList.Count > 0)
            {
                if (Clipboard.ContainsData("PersistableScheduleData"))
                {
                    using (var pasteAction = new SchedulePasteAction(options, Presenter.LockManager, Presenter.SchedulePartFilter))
                    {

                        undoRedo.CreateBatch(Resources.UndoRedoPaste);
                        splitAbsences(SelectedSchedules());
                        try
                        {
                            IList<IScheduleDay> pasteList =
                                   GridHelper.HandlePasteScheduleGridFrozenColumn(_grid, Presenter.ClipHandlerSchedule, pasteAction);

	                        if (!pasteList.IsEmpty())
	                        {
		                        var absenceMerger = new AbsenceMerger(pasteList);
								absenceMerger.MergeWithDayBefore();
								absenceMerger.MergeOnDayStart();
		                        Presenter.TryModify(pasteList);
	                        }

	                        undoRedo.CommitBatch();

                            if (!Presenter.ClipHandlerSchedule.IsInCutMode)
                                OnPasteCompleted();

                            InvalidateSelectedRows(new List<IScheduleDay> { Presenter.ClipHandlerSchedule.ClipList[0].ClipValue });
                        }
                        catch (DayOffOutsideScheduleException dayOffOutside)
                        {
                            undoRedo.RollbackBatch();
                            ShowErrorMessage(dayOffOutside.Message, Resources.DayOffOutsideScheduleException);
                        }
						catch (ValidationException validationException)
						{
							undoRedo.RollbackBatch();
							ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, validationException.Message), Resources.ValidationError);
						}
                    }
                }
            }
        }

	    private void splitLongAbsencePeriod(IScheduleDay splitDay)
        {
            IList<IScheduleDay> modifiedParts = new List<IScheduleDay>();
            IList<IPersonAbsence> dayAbsence = getSplitDayLongAbsences(splitDay);

            foreach (IPersonAbsence personAbsence in splitDay.PersonAbsenceCollection())
            {
                IList<IPersonAbsence> splits = personAbsence.Split(splitDay.Period);

                //split long absences, will remove the long absence on splitday
                foreach (IPersonAbsence personAbsenceSplitPart in splits)
                {
                    IScheduleDay splitPart = Presenter.SchedulerState.Schedules[personAbsence.Person].ScheduledDay(personAbsenceSplitPart.Period.ToDateOnlyPeriod(splitDay.TimeZone).StartDate);
                    IScheduleDay cloneSplitPart = (IScheduleDay)splitPart.Clone();

                    splitPart.Clear<IPersonAbsence>();
                    splitPart.Add(personAbsenceSplitPart);
                    foreach (IPersonAbsence personAbsenceCloneDay in cloneSplitPart.PersonAbsenceCollection(true))
                    {
                        if (personAbsenceCloneDay.Layer.Payload.Id != personAbsenceSplitPart.Layer.Payload.Id)
                            splitPart.Add(personAbsenceCloneDay);
                    }

                    modifiedParts.Add(splitPart);
                    Presenter.TryModify(modifiedParts);
                    modifiedParts.Clear();
                }
            }

            //put saved absences back on splitday
            foreach (IPersonAbsence abs in dayAbsence)
            {
                IScheduleDay pasteDay = Presenter.SchedulerState.Schedules[splitDay.Person].ScheduledDay(splitDay.Period.ToDateOnlyPeriod(splitDay.TimeZone).StartDate);
                pasteDay.Add(abs);
                modifiedParts.Add(pasteDay);
                Presenter.TryModify(modifiedParts);
                modifiedParts.Clear();
            }
        }

        private static IList<IPersonAbsence> getSplitDayLongAbsences(IScheduleDay splitDay)
        {
        	IList<IPersonAbsence> dayAbsences = new SplitLongDayAbsences().SplitAbsences(splitDay);
            return dayAbsences;
        }

        private void splitAbsences(IList<IScheduleDay> selectedParts)
        {
            foreach (IScheduleDay part in selectedParts)
            {
                IScheduleDay day = Presenter.SchedulerState.Schedules[part.Person].ScheduledDay(part.Period.ToDateOnlyPeriod(part.TimeZone).StartDate);
                splitLongAbsencePeriod(day);
            }
        }

        public void OnPasteCompleted()
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<EventArgs> temp = ViewPasteCompleted;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        public void ValidatePersons(IEnumerable<IPerson> listPersons)
        {
            Presenter.SchedulerState.Schedules.ValidateBusinessRulesOnPersons(listPersons,
                                                                              TeleoptiPrincipal.Current.Regional.Culture,
																					Presenter.SchedulerState.SchedulingResultState.GetRulesToRun());
        }

        public  virtual void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules)
        {
            foreach (IScheduleDay schedulePart in schedules)
            {
                _grid.InvalidateRange(GridRangeInfo.Row(GetRowForAgent(schedulePart.Person)));
            }
        }

        /// <summary>
        /// calculate the width of column headers
        /// </summary>
        /// <returns></returns>
        public int CalculateColHeadersWidth()
        {
            int width = 0;

            for (int i = 0; i < (int)ColumnType.StartScheduleColumns; i++)
            {
                width += _grid.ColWidths[i];
            }
            return width;
        }

        public virtual void SetSelectedDateLocal(DateOnly dateOnly)
        {
            int column = GetColumnForDate(dateOnly);
            if(TheGrid.CurrentCell.ColIndex != column)
            {
				TheGrid.CurrentCell.MoveTo(TheGrid.CurrentCell.RowIndex, column);
            	TheGrid.CurrentCell.ScrollInView(GridScrollCurrentCellReason.MoveTo);
            }
                
        }

        //get the local selected date
        public virtual DateOnly SelectedDateLocal()
        {
            DateOnly tag;
            if (_grid.CurrentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
            {
                tag = (DateOnly) _grid.Model[1, _grid.CurrentCell.ColIndex].Tag; 
            }
            else
            {
                tag = Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;
            }
            
            return tag;
        }

        public IList<IScheduleDay> DeleteList<T>(ClipHandler<T> clipHandler)
        {
            IList<IScheduleDay> schedulesForDelete = new List<IScheduleDay>();

            foreach (Clip<T> clip in clipHandler.ClipList)
            {
                int row = clipHandler.AnchorRow + clip.RowOffset;
                int col = clipHandler.AnchorColumn + clip.ColOffset;

                IScheduleDay scheduleRange = (IScheduleDay)_grid.Model[row, col].CellValue;

                if(scheduleRange.SignificantPart() != SchedulePartView.None)
                    schedulesForDelete.Add(scheduleRange);
            }

            return schedulesForDelete;
        }

        public IList<IScheduleDay> DeleteList<T>(ClipHandler<T> clipHandler,DeleteOption deleteOption )
        {
            IList<IScheduleDay> schedulesForDelete = new List<IScheduleDay>();

            foreach (Clip<T> clip in clipHandler.ClipList)
            {
                int row = clipHandler.AnchorRow + clip.RowOffset;
                int col = clipHandler.AnchorColumn + clip.ColOffset;

                IScheduleDay scheduleRange = (IScheduleDay)_grid.Model[row, col].CellValue;

                if (deleteOption.OvertimeAvailability)
                    schedulesForDelete.Add(scheduleRange);
                else if (scheduleRange.SignificantPart() != SchedulePartView.None )
                    schedulesForDelete.Add(scheduleRange);
                 

            }

            return schedulesForDelete;
        }

        /// <summary>
        /// Gets a list with selected schedules for current column
        /// </summary>
        /// <returns></returns>
        public IList<IScheduleDay> CurrentColumnSelectedSchedules()
        {
            GridRangeInfoList rangeList = GridHelper.GetGridSelectedRanges(_grid, true);
            IList<IScheduleDay> selectedSchedules = new List<IScheduleDay>();

            foreach (GridRangeInfo range in rangeList)
            {
                if (range.IsCells)
                {
                    AddSelectedSchedulesInColumnToList(range, range.Left, selectedSchedules);
                }
            }

            return selectedSchedules;
        }

        /// <summary>
        /// Gets a list of the current selected schedules.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-26
        /// </remarks>
        public IList<IScheduleDay> SelectedSchedules()
        {
            GridRangeInfoList rangeList = GridHelper.GetGridSelectedRanges(_grid, true);
            IList<IScheduleDay> selectedSchedules = new List<IScheduleDay>();

            foreach (GridRangeInfo range in rangeList)
            {
                if (range.IsCells)
                {
                    for (int i = range.Left; i <= range.Right; i++)
                    {
                        AddSelectedSchedulesInColumnToList(range, i, selectedSchedules);
                    }
                }
            }

            return selectedSchedules;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<IList<IScheduleDay>> SelectedSchedulesPerEqualTwoRanges()
		{
			var gridRangeInfoList = GridHelper.GetGridSelectedRanges(_grid, true);
			IList<IList<IScheduleDay>> list = new List<IList<IScheduleDay>>();

			if(gridRangeInfoList.Count == 2 && GridHelper.IsRangesSameSize(gridRangeInfoList[0], gridRangeInfoList[1]))
			{
				if (gridRangeInfoList[0].IsCells && gridRangeInfoList[1].IsCells)
				{
					foreach (GridRangeInfo gridRangeInfo in gridRangeInfoList)
					{
						IList<IScheduleDay> scheduleDays = new List<IScheduleDay>();

						for (var i = gridRangeInfo.Left; i <= gridRangeInfo.Right; i++)
						{
							AddSelectedSchedulesInColumnToList(gridRangeInfo, i, scheduleDays);
						}

						list.Add(scheduleDays);	
					}
				}
			}

			return list;
		}

		public IList<DateOnly> LockedDates(IGridlockManager gridlockManager)
		{
			if(gridlockManager == null)
				throw new ArgumentNullException("gridlockManager");

			IList<DateOnly> lockedDates = new List<DateOnly>();

			foreach (var scheduleDay in SelectedSchedules())
			{
				var lockDictionary = gridlockManager.Gridlocks(scheduleDay);

				if (lockDictionary != null && lockDictionary.Count != 0 && !lockedDates.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly))
					lockedDates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);

			}

			return lockedDates;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<IPerson, IList<DateOnly>> LockedDatesOnPerson(IGridlockManager gridlockManager)
		{
			if(gridlockManager == null)
				throw new ArgumentNullException("gridlockManager");

			IDictionary<IPerson, IList<DateOnly>> locks = new Dictionary<IPerson, IList<DateOnly>>();

			foreach (var scheduleDay in SelectedSchedules())
			{
				var writeProtectLock = false;
				var otherLock = false;

				var lockDictionary = gridlockManager.Gridlocks(scheduleDay);
				if (lockDictionary == null || lockDictionary.Count == 0) continue;

				foreach (var gridLock in lockDictionary)
				{
					if (gridLock.Value.LockType == LockType.WriteProtected)
					{
						if(!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
							writeProtectLock = true;	
					}
					else
					{
						otherLock = true;
					}
				}

				if (!writeProtectLock && !otherLock) continue;

				if(locks.ContainsKey(scheduleDay.Person))
				{
					locks[scheduleDay.Person].Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
				}
				else
				{
					IList<DateOnly> lockeDates = new List<DateOnly>{scheduleDay.DateOnlyAsPeriod.DateOnly};
					locks.Add(scheduleDay.Person, lockeDates);
				}
			}

			return locks;
		}
		

        public void SetSelectionFromParts(IList<IScheduleDay> scheduleParts)
        {
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            int minCol = int.MaxValue;
            int maxCol = int.MinValue;

            foreach (IScheduleDay part in scheduleParts)
            {
                Point point = GetCellPositionForAgentDay(part.Person, part.DateOnlyAsPeriod.DateOnly.Date);

                if (point.Y > 0 && point.X > 0)
                {
                    if (point.Y < minRow)
                        minRow = point.Y;
                    if (point.Y > maxRow)
                        maxRow = point.Y;
                    if (point.X < minCol)
                        minCol = point.X;
                    if (point.X > maxCol)
                        maxCol = point.X;
                }
            }

            if (minRow > 0 && minCol > 0 && minRow != int.MaxValue && minCol != int.MaxValue)
            {
                GridRangeInfo info = GridRangeInfo.Cells(minRow, minCol, maxRow, maxCol);

                TheGrid.Selections.Clear(true);
                TheGrid.CurrentCell.Activate(minRow, minCol, GridSetCurrentCellOptions.SetFocus);
                TheGrid.Selections.ChangeSelection(info, info, true);
                //TheGrid.CurrentCell.MoveTo(minRow, minCol, GridSetCurrentCellOptions.None);
                TheGrid.CurrentCell.MoveTo(minRow, minCol, GridSetCurrentCellOptions.ScrollInView);
            }
            else
            {
                SelectFirstDayInGrid();  
            }
        }

        public virtual void SelectFirstDayInGrid()
        {
            GridRangeInfo info = GridRangeInfo.Cell(TheGrid.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns);
            TheGrid.Selections.Clear(true);
            TheGrid.CurrentCell.Activate(TheGrid.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns, GridSetCurrentCellOptions.SetFocus);
            TheGrid.Selections.ChangeSelection(info, info, true);
            TheGrid.CurrentCell.MoveTo(TheGrid.Rows.HeaderCount + 1, (int)ColumnType.StartScheduleColumns);
        }

        public virtual ICollection<DateOnly> AllSelectedDates()
        {
            ICollection<DateOnly> ret = new HashSet<DateOnly>();
            foreach (IScheduleDay part in SelectedSchedules())
            {
                DateOnly dateOnly = part.DateOnlyAsPeriod.DateOnly;
                ret.Add(dateOnly);
            }
            return ret;
        }

	    public static IEnumerable<IPerson> AllSelectedPersons(IEnumerable<IScheduleDay> selectedSchedules)
        {
            var extractor = new PersonListExtractorFromScheduleParts(selectedSchedules);
            return extractor.ExtractPersons();
        }

        public virtual IEnumerable<IPerson> AllSelectedPersons()
        {
            return AllSelectedPersons(SelectedSchedules());
        }

        public virtual void AddSelectedSchedulesInColumnToList(GridRangeInfo range, int colIndex, ICollection<IScheduleDay> selectedSchedules)
        {
            for (int j = range.Top; j <= range.Bottom; j++)
            {
                if (j - (RowHeaders + 1) < 0)
                    continue;

                if (colIndex < (int) ColumnType.StartScheduleColumns)
                    continue;

                if (_grid.Model[1, colIndex].Tag == null)
                    continue;

                var localDate = (DateOnly)_grid.Model[1, colIndex].Tag;
                IPerson agent = null;
                if (j - (RowHeaders + 1) >= Presenter.SchedulerState.FilteredPersonDictionary.Count)
                {
                    if (Presenter.SchedulerState.FilteredPersonDictionary.Count > 0)
                        agent = Presenter.SchedulerState.FilteredPersonDictionary.ElementAt(0).Value;
                }
                else
                    agent = Presenter.SchedulerState.FilteredPersonDictionary.ElementAt(j - (RowHeaders + 1)).Value;

                if (agent != null)
                {
                    IScheduleDay schedulePart =
                        Presenter.SchedulerState.Schedules[agent].ScheduledDay(localDate);
                    selectedSchedules.Add(schedulePart);
                }
                else
                    selectedSchedules.Clear();
            }
        }

        /// <summary>
        /// Get list with persons for supposed destination when pasting
        /// </summary>
        /// <returns></returns>
        public IList<IPerson> PersonsInDestination()
        {
            IList<int> rowList = new List<int>();

            GridRangeInfoList rangeList = GridHelper.GetGridSelectedRanges(_grid, true);
            IList<IPerson> list = new List<IPerson>();
            int baseRow = rangeList.ActiveRange.Top;

            for(int i = rangeList.ActiveRange.Top; i <= rangeList.ActiveRange.Bottom; i++)
            {
                rowList.Add(i);
            }

            foreach (Clip<IScheduleDay> clip in Presenter.ClipHandlerSchedule.ClipList)
            {
                if((clip.RowOffset + baseRow <= _grid.RowCount) && 
                    (clip.RowOffset + baseRow > ColHeaders))
                {
                    if(!rowList.Contains(clip.RowOffset + baseRow))
                        rowList.Add(clip.RowOffset + baseRow);
                }
            }

            foreach(int i in rowList)
                list.Add(Presenter.SchedulerState.FilteredPersonDictionary[((IPerson)_grid.Model[i, 1].Tag).Id.Value]);

            return list;
        }


        public int GetRowForAgent(IEntity person)
        {
            int retRow = -1;
            for (int row = ColHeaders; row <= _grid.RowCount; row++)
            {
                IScheduleDay scheduleDay = _grid.Model[row, (int)ColumnType.StartScheduleColumns].CellValue as IScheduleDay;
                if (scheduleDay != null)
                {
                    if (scheduleDay.Person.Equals(person))
                    {
                        retRow = row;
                        break;
                    }
                }
            }

            return retRow;
        }

        public int GetColumnForDate(DateTime date)
        {
            int retCol = -1;

            int col = (int)ColumnType.StartScheduleColumns;
            var scheduleDay = _grid.Model[ColHeaders + 1, col].CellValue as IScheduleDay;
            if (scheduleDay != null)
            {
                var startDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
                TimeSpan days = date.Subtract(startDate);
                retCol = col + (int)days.TotalDays;
            }


            //for (int col = (int)ColumnType.StartScheduleColumns; col <= _grid.ColCount; col++)
            //{
            //    IScheduleDay scheduleDay = _grid.Model[ColHeaders + 1, col].CellValue as IScheduleDay;
            //    if (scheduleDay != null &&
            //        scheduleDay.Period.Contains(date))
            //    {
            //        retCol = col;
            //        break;
            //    }
            //}

            return retCol;
        }

        public virtual Point GetCellPositionForAgentDay(IEntity person, DateTime dayDate)
        {
            return new Point(GetColumnForDate(dayDate), GetRowForAgent(person));
        }

        #region IDisposable Members

        public void Dispose()
        {
            _isDesposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
            return;
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
// ReSharper disable InconsistentNaming
        private void Dispose(bool disposing)
// ReSharper restore InconsistentNaming
        {
            if (disposing)
            {
                ReleaseManagedResources();
                
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            setEventHandlers(false);
            _cellFontSmall.Dispose();
            _cellFontBig.Dispose();
            _fontTimeLine.Dispose();
            Presenter = null;
            ((HandleBusinessRuleResponse)_handleBusinessRuleResponse).Dispose();
        }
        #endregion

        #region IHelpContext Members

        public bool HasHelp
        {
            get { return true; }
        }

        public string HelpId
        {
            get { return GetType().Name; }
        }

        #endregion

        public IHandleBusinessRuleResponse HandleBusinessRuleResponse
        {
            get { return _handleBusinessRuleResponse; }
        }

        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>The grid.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-12-04    
        /// /// </remarks>
        public GridControl TheGrid
        {
            get { return _grid; }
        }

        /// <summary>
        /// Refresh range on Person, Period
        /// </summary>
        /// <param name="person"></param>
        /// <param name="period"></param>
        public void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period)
        {
            //thread stuff
            if (_grid.InvokeRequired)
            {
                RefreshRangeForAgentPeriodHandler handler = RefreshRangeForAgentPeriod;
                _grid.Invoke(handler, new object[] {person, period});
            }
            else
            {
                int row = GetRowForAgent(person);
                _grid.RefreshRange(GridRangeInfo.Cell(row, (int)ColumnType.RowHeaderColumn + 1), true);
                //loop for all days in period
                for (DateTime date = period.StartDateTime; date < period.EndDateTime; date = date.AddDays(1))
                {
                    int column = GetColumnForDate(date);
                    if (column >= 0 && row >= 0)
                    {
                        _grid.RefreshRange(GridRangeInfo.Cell(row, column), true);

                        //check if selection in grid contains cell
                        if (_grid.Selections.Ranges.Contains(GridRangeInfo.Cell(row, column)))
                        {
                            //update selection info
                        	var selectionHandler = RefreshSelectionInfo;
							if (selectionHandler != null) selectionHandler(this, EventArgs.Empty);

                            //check if cell = current cell in grid
                            if (_grid.CurrentCell.ColIndex == column && _grid.CurrentCell.RowIndex == row)
                            {
                                //update shift editor
                            	var editorHandler = RefreshShiftEditor;
                                if (editorHandler != null) editorHandler(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            }
        }

		public void UpdateShiftEditor()
		{
			var editorHandler = RefreshShiftEditor;
			if (editorHandler != null) editorHandler(this, EventArgs.Empty);
		} 
    }
}
