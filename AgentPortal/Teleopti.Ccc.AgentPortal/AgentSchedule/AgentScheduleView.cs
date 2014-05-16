using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Schedule;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Schedule;
using Teleopti.Ccc.AgentPortal.Common.Controls;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.ScheduleControlDataProvider;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    public class AgentScheduleView : AgentScheduleViewBase<AgentSchedulePresenter>
    {
        /// <summary>
        /// Host schedule Control
        /// </summary>
        private readonly CustomScheduleControl  _scheduleControl;

	    private readonly ILegendLoader _legendLoader;
	    private DateTimePeriodDto _loadedPeriod;

        /// <summary>
        /// Gets the schedule control host.
        /// </summary>
        /// <value>The schedule control host.</value>
        public CustomScheduleControl ScheduleControlHost
        {
            get { return _scheduleControl; }
        }

        /// <summary>
        /// Refreshes the specified reload data.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
           
            _scheduleControl.Cursor = Cursors.WaitCursor;
            SetDataSource();
            _scheduleControl.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Sets the resolution.
        /// </summary>
        /// <param name="resolution"></param>
        public override void SetResolution(int resolution)
        {
            base.SetResolution(resolution);

            if (resolution > 0)
            {
                _scheduleControl.Appearance.DivisionsPerHour = resolution;
            }
        }

		/// <summary>
		/// Gets current resolution in schedule.
		/// </summary>
		/// <value>The resolution of schedule control.</value>
    	public int Resolution
    	{
			get { return _scheduleControl.Appearance.DivisionsPerHour; }
    	}
        
        /// <summary>
        /// Initializes the schedule control.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void InitializeScheduleControl()
        {
            // set static strings
            ScheduleGrid.DisplayStrings[4] = "";//hide the text aprea in all day area in day view
            ScheduleGrid.DisplayStrings[3] = string.Concat(UserTexts.Resources.Starting, " ");

            // set Visual style to Office2007Blue look
            _scheduleControl.Appearance.VisualStyle = GridVisualStyles.Office2007Blue;// ColorHelper.AgentPortalScheduleCtrlVisualStyle;//GridVisualStyles.Office2007Silver;

            // set drag behaviour
            _scheduleControl.AllowAdjustAppointmentsWithMouse = true;

            // Set Month view format
            _scheduleControl.Appearance.MonthShowFullWeek = true;
            _scheduleControl.Appearance.WorkWeekHeaderFormat = "d dddd";

            // Set Calendar Schedule Date Selection Method
            _scheduleControl.Calendar.CalenderGrid.AllowSelection = GridSelectionFlags.Any;

            // set Appointment Visualiztion styles
            _scheduleControl.ShowRoundedCorners = true;
            _scheduleControl.Appearance.ClickItemBorderColor = Color.DarkRed; //ColorHelper.ScheduleControlClickedItemBorder;
            _scheduleControl.Appearance.PrimeTimeStart = 9;
            _scheduleControl.Appearance.PrimeTimeEnd = 17;
            ScheduleGrid.SpanOnlyPrimeTime = true;
            LanguageAndCulture();

            // set Resolution
            var resolution = 4;
            try
            {
                resolution = AgentPortalSettingsHelper.Resolution;
            }
            catch (Exception) {}
            
            SetResolution(resolution);

            // Set Navigation panel and Calendar options
            _scheduleControl.NavigationPanelPosition =CalendarNavigationPanelPosition.Left;
            _scheduleControl.NavigationPanelFillWithCalendar = true;
            _scheduleControl.Calendar.DateValue = DateTime.Now;
            _scheduleControl.ScheduleType = ScheduleViewType.Week;
            _scheduleControl.Calendar.ShowWeekNumbers = true;
        }

        private void LanguageAndCulture() 
        {
            // Set Culture 
            PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            CultureInfo cultureUi = (person.UICultureLanguageId.HasValue
                                        ? CultureInfo.GetCultureInfo(person.UICultureLanguageId.Value)
										: CultureInfo.CurrentUICulture).FixPersianCulture(); 
            CultureInfo cultureFormat = (person.CultureLanguageId.HasValue
                                            ? CultureInfo.GetCultureInfo(person.CultureLanguageId.Value)
											: CultureInfo.CurrentCulture).FixPersianCulture();

            CultureInfo newCulture = (CultureInfo)cultureUi.Clone();
            newCulture.DateTimeFormat = (DateTimeFormatInfo) cultureFormat.DateTimeFormat.Clone();
            _scheduleControl.Culture = newCulture;

            // Set Time line format
            bool is24HourFormat = newCulture.DateTimeFormat.ShortTimePattern.Contains("H");

            _scheduleControl.Appearance.Hours24 = is24HourFormat;
            _scheduleControl.Appearance.NavigationCalendarStartDayOfWeek =
                        newCulture.DateTimeFormat.FirstDayOfWeek;
            _scheduleControl.Appearance.WeekCalendarStartDayOfWeek =
                        newCulture.DateTimeFormat.FirstDayOfWeek;
            _scheduleControl.Appearance.MonthCalendarStartDayOfWeek =
                        newCulture.DateTimeFormat.FirstDayOfWeek;
			_scheduleControl.Calendar.CalenderGrid.Model.TableStyle.CultureInfo = cultureUi;
			_scheduleControl.ISO8601CalenderFormat = Iso8601Helper.UseIso8601Format(cultureUi);
        }

        /// <summary>
        /// Sets the schedule control event handlers.
        /// </summary>
        public void SetScheduleControlEventHandlers()
        {
            _scheduleControl.AdjustingAppointmentWithMouse += AdjustingAppointmentWithMouse;
            _scheduleControl.ShowingAppointmentForm +=ShowingAppointmentForm;
            _scheduleControl.ScheduleAppointmentClick +=ScheduleAppointmentClick;
            _scheduleControl.SetupContextMenu +=SetupContextMenu;
            _scheduleControl.DeleteScheduleAppointment += DeleteScheduleItem;
            _scheduleControl.Calendar.ContextMenu = new ContextMenu(); //Fake menu so that we can get rid of events on the calander
        }

        /// <summary>
        /// Sets the data source.
        /// </summary>
        public void SetDataSource()
        {
            // set the data source
            _scheduleControl.DataSource = new CustomScheduleDataProvider(this,_legendLoader);
            // Set the property to not to save the changes
            _scheduleControl.DataSource.SaveOnCloseBehaviorAction = SaveOnCloseBehavior.DoNotSave;
        }

        /// <summary>
        /// Sets the color theme.
        /// </summary>
        /// <param name="colorTheme">The color theme.</param>
        public override void SetColorTheme(ScheduleAppointmentColorTheme colorTheme) 
        {
            base.SetColorTheme(colorTheme);
            Refresh();
        }

        /// <summary>
        /// Deletes the schedule appointments.
        /// </summary>
        public void DeleteScheduleAppointments()
        {
            if (_scheduleControl.ClickedScheduleAppointment != null)
            {
                DialogResult result =
                    MessageBoxHelper.ShowConfirmationMessage(UserTexts.Resources.AreYouSureYouWantToDelete,
                                                             UserTexts.Resources.AgentPortal);

                if (result == DialogResult.Yes)
                {
                    if (_scheduleControl.ClickedScheduleAppointment.AppointmentType == ScheduleAppointmentTypes.Request)
                    {
                        var dto = _scheduleControl.ClickedScheduleAppointment.Tag as PersonRequestDto;
                        if(dto!=null)
                        {
                            if (dto.CanDelete)
                                SdkServiceHelper.SchedulingService.DeletePersonRequest(dto);
                        }
                    }
                }
                Refresh();
            }
        }

        /// <summary>
        /// Loads the agent schedules.
        /// </summary>
        /// <param name="period">The period.</param>
        public void LoadAgentSchedules(DateTimePeriodDto period)
        {
            _loadedPeriod = period;
            Presenter.LoadAgentSchedules(period);
        }

        public DateOnlyPeriod LoadedPeriod
        {
            get
            {
                //ola 14387 because of a fix for umnia (DST problem) where we have removed 1 second from localstartdatetime, we add 2 days instead
                return new DateOnlyPeriod(new DateOnly(_loadedPeriod.LocalStartDateTime).AddDays(2),new DateOnly(_loadedPeriod.LocalEndDateTime));
            }
        }

        /// <summary>
        /// Loads the schedule messenger schedule.
        /// </summary>
        /// <param name="date">The date.</param>
        public void LoadScheduleMessengerSchedule(DateTime date)
        {
             Presenter.LoadScheduleMessengerSchedule(date);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentScheduleView"/> class.
        /// </summary>
        protected AgentScheduleView()
        {
        }

	    /// <summary>
	    /// Initializes a new instance of the <see cref="AgentScheduleView"/> class.
	    /// </summary>
	    /// <param name="scheduleControl">The schedule control.</param>
	    /// <param name="scheduleStateHolder">The schedule state holder.</param>
	    /// <param name="clipHandler">The clip handler.</param>
	    /// <param name="legendLoader"></param>
	    public AgentScheduleView(CustomScheduleControl scheduleControl, AgentScheduleStateHolder scheduleStateHolder, ILegendLoader legendLoader)
             : this()
        {
            Presenter = new AgentSchedulePresenter(this, scheduleStateHolder);
            _scheduleControl = scheduleControl;
		    _legendLoader = legendLoader;
        }

        /// <summary>
        /// Adjustings the appointment with mouse.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Schedule.AdjustingAppointmentMouseWithEventArgs"/> instance containing the event data.</param>
        private void AdjustingAppointmentWithMouse(object sender, AdjustingAppointmentMouseWithEventArgs e)
        {
            _scheduleControl.ClickedScheduleAppointment = (CustomScheduleAppointment)e.Item;

            if (!_scheduleControl.ClickedScheduleAppointment.AllowDrag)
            {
                e.Cancel = true;
            }
        }


        /// <summary>
        /// Showings the appointment form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Schedule.ShowingAppointFormEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        private void ShowingAppointmentForm(object sender, ShowingAppointFormEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Schedules the appointment click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Schedule.ScheduleAppointmentClickEventArgs"/> instance containing the event data.</param>
        private void ScheduleAppointmentClick(object sender, ScheduleAppointmentClickEventArgs e)
        {
            if (_scheduleControl.ClickedScheduleAppointment != null)
            {
                if (e.ClickType == ScheduleAppointmentClickType.LeftDblClick)
                {
                    switch (_scheduleControl.ClickedScheduleAppointment.AppointmentType)
                    {
                        case ScheduleAppointmentTypes.Request:
                            //Disabled for now
                            //PersonRequestDto personRequestDto = _scheduleControl.ClickedScheduleAppointment.Tag as PersonRequestDto;
                            //RequestHelper.ShowRequestScreen(personRequestDto, _scheduleControl.ParentForm);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Setups the context menu.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private static void SetupContextMenu(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;  // This is to override the default context menu
        }

        /// <summary>
        /// Deletes the schedule item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Teleopti.Ccc.AgentPortalCode.Common.CustomScheduleAppointmentDeleteEventArgs"/> instance containing the event data.</param>
        private void DeleteScheduleItem(object sender, CustomScheduleAppointmentDeleteEventArgs e)
        {
            DeleteScheduleAppointments();
        }
    }
}
