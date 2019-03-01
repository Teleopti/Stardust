using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
    /// <summary>
    /// Base view class
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"),System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class IntradayScheduleView : AddScheduleLayers, IScheduleViewBase, IDisposable
    {
        private IIntradayView _intradayView;
        private IHandleBusinessRuleResponse _handleBusinessRuleResponse = new HandleBusinessRuleResponse();
    	private GridControl _gridControl;
        public event EventHandler<EventArgs> ViewPasteCompleted;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IntradayScheduleView(GridControl gridControl, IIntradayView intradayView, ISchedulerStateHolder schedulerState,
            GridlockManager lockManager, SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer)
            : base(null)
		{
			_gridControl = gridControl;
            _intradayView = intradayView;
            Presenter = new DayPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer);
        }

        public bool IsOverviewColumnsHidden { get; set; }
        
        /// <summary>
        /// Number of rowheaders
        /// </summary>
        public int RowHeaders
        {
            get { return 0; }
        }

        /// <summary>
        /// Number of colheaders
        /// </summary>
        public int ColHeaders
        {
            get { return 0; }
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
                return (_intradayView.RightToLeft == RightToLeft.Yes);
            }
        }

        //return tip for a day header
        public string DayHeaderTooltipText(GridStyleInfo gridStyle,DateOnly currentDate)
        {
            return string.Empty;
        }

        public void SetCellBackTextAndBackColor(GridQueryCellInfoEventArgs e, DateOnly dateTime, bool backColor, bool textColor, IScheduleDay schedulePart)
        {
            
        }

        public void OnPasteCompleted()
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<EventArgs> temp = ViewPasteCompleted;
            if (temp != null)
                temp(this, new EventArgs());
        }

        public void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo)
        {
        }

        public ICollection<DateOnly> AllSelectedDates()
        {
            return new List<DateOnly>();
        }

	    public ICollection<DateOnly> AllSelectedDates(IEnumerable<IScheduleDay> selectedSchedules)
	    {
			return new List<DateOnly>();
	    }

	    public void InvalidateSelectedRow(IScheduleDay schedulePart)
	    {
		    RefreshRangeForAgentPeriod(schedulePart.Person, new DateTimePeriod());
	    }

		public ITimeZoneGuard TimeZoneGuard { get; }

		/// <summary>
        /// Gets a list with selected schedules for current column
        /// </summary>
        /// <returns></returns>
        public IList<IScheduleDay> CurrentColumnSelectedSchedules()
        {
            return new List<IScheduleDay>();
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
            IList<IScheduleDay> selectedSchedules = new List<IScheduleDay>();
            return selectedSchedules;
        }

        #region IDisposable Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void dispose(bool disposing)
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
            if (_handleBusinessRuleResponse!=null)
            {
                ((IDisposable)_handleBusinessRuleResponse).Dispose();
                _handleBusinessRuleResponse = null;
            }
            _intradayView = null;
                //_rules = null;
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
        public GridControl ViewGrid
        {
            get { return _gridControl; }
        }

        /// <summary>
        /// Refresh range on Person, Period
        /// </summary>
        /// <param name="person"></param>
        /// <param name="period"></param>
        public void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period)
        {
            if (_intradayView.InvokeRequired)
            {
                RefreshRangeForAgentPeriodHandler handler = RefreshRangeForAgentPeriod;
                _intradayView.BeginInvoke(handler, person, period);
            }
            else
            {
                _intradayView.RefreshPerson(person as IPerson);
            }
        }

        private delegate void RefreshRangeForAgentPeriodHandler(IEntity person, DateTimePeriod period);

      
    }
}
