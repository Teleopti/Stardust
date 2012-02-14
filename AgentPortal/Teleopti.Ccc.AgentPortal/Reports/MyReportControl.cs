#region Imports

using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.AgentPortal.Reports
{
    /// <summary>
    /// Visualize my report
    /// </summary>
    /// <remarks>
    /// Created by: Madhuranga Pinnagoda
    /// Created date: 2008-10-10
    /// </remarks>
    public partial class MyReportControl : BaseUserControl
    {
        #region Fields

        private static MyReportStateHolder _stateHolder;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MyReportControl"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-13
        /// </remarks>
        public MyReportControl()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                _stateHolder = new MyReportStateHolder();
                StateHolder.SelectedDateTimePeriodDto = new DateTimePeriodDto();
                navigationMonthCalendarMyReport.SetCurrentPersonCulture();
                navigationMonthCalendarMyReport.DateValue = DateTime.Today;
            }
        }

        #endregion

        #region MyReportStateHolderProperties

        /// <summary>
        /// Gets the state holder.
        /// </summary>
        /// <value>The state holder.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-13
        /// </remarks>
        public static MyReportStateHolder StateHolder
        {
            get { return _stateHolder; }
        }

        #endregion

        private DateTime startDateForScheduleInfo;
        private DateTime endDateForScheduleInfo;

        /// <summary>
        /// Handles the DateValueChanged event of the navigationMonthCalendarMyReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        private void navigationMonthCalendarMyReport_DateValueChanged(object sender, EventArgs e)
        {
            HandleDateChange();
        }

        /// <summary>
        /// Handles the date change.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        private void HandleDateChange()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (navigationMonthCalendarMyReport.SelectedDates != null)
            {
                if (navigationMonthCalendarMyReport.SelectedDates.Count == 0)
                {
                    startDateForScheduleInfo = navigationMonthCalendarMyReport.DateValue.Date;
                }
                else if (navigationMonthCalendarMyReport.SelectedDates.Count > 0)
                {
                    startDateForScheduleInfo = navigationMonthCalendarMyReport.SelectedDates[0].Date;
                }

                startDateForScheduleInfo = DateHelper.GetFirstDateInWeek(startDateForScheduleInfo, CultureInfo.CurrentCulture);
                endDateForScheduleInfo = startDateForScheduleInfo.AddDays(6);
                SetSelectedDateTime();

                StateHolder.LoadMyReportData();
                myReportScheduleInfoControl.InitializeScheduleInfoControl();
                //Refresh Info panelInitializeScheduleGrid
                myReportInfoGridControl1.RefreshControl();
                //Refresh Queue panel
                myReportQueueControl1.RefreshControl();
            }

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Handles the Load event of the MyReportControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        private void MyReportControl_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            navigationMonthCalendarMyReport.SelectedDates.SelectionsChanged +=
                navigationMonthCalendarMyReport_DateValueChanged;

            startDateForScheduleInfo = DateHelper.GetFirstDateInWeek(DateTime.Today, CultureInfo.CurrentCulture);
            endDateForScheduleInfo = startDateForScheduleInfo.AddDays(6);
            SetSelectedDateTime();

            StateHolder.LoadMyReportData();
            myReportScheduleInfoControl.InitializeScheduleInfoControl();
            myReportInfoGridControl1.InitializeScheduleInfoControl();
            myReportQueueControl1.RefreshControl();

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Sets the selected date time.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/13/2008
        /// </remarks>
        private void SetSelectedDateTime()
        {
            if (StateHolder != null)
            {
                StateHolder.SelectedDateTimePeriodDto.UtcStartTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(startDateForScheduleInfo);
                StateHolder.SelectedDateTimePeriodDto.UtcEndTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(endDateForScheduleInfo);
                StateHolder.SelectedDateTimePeriodDto.UtcStartTimeSpecified = true;
                StateHolder.SelectedDateTimePeriodDto.UtcEndTimeSpecified = true;
                StateHolder.SelectedDateTimePeriodDto.LocalStartDateTime = startDateForScheduleInfo;
                StateHolder.SelectedDateTimePeriodDto.LocalEndDateTime = endDateForScheduleInfo;
                StateHolder.SelectedDateTimePeriodDto.LocalStartDateTimeSpecified= true;
                StateHolder.SelectedDateTimePeriodDto.LocalEndDateTimeSpecified = true;
            }
        }
    }
}