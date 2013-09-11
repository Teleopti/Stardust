using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortal.AgentSchedule;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Main;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    /// <summary>
    /// AgentScheduleMessenger main window
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 6/23/2008
    /// </remarks>
    public partial class ScheduleMessengerScreen : BaseRibbonForm
    {
        private readonly MainScreen _caller;
        private readonly AgentScheduleView _scheduleView;
        private readonly PushMessageController _controller;
        private readonly MessageBrokerHandler _messageBrokerHandler;
        private bool _updated;
    	private int _scheduleLoadRetryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleMessengerScreen"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-27
        /// </remarks>
        private ScheduleMessengerScreen()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleMessengerScreen"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-01
        /// </remarks>
        public ScheduleMessengerScreen(MainScreen parent, AgentScheduleView scheduleView, PushMessageController pushMessageController)
            : this()
        {
            _controller = pushMessageController;
            _caller = parent;
            _scheduleView = scheduleView;

            _messageBrokerHandler = new MessageBrokerHandler(this, _controller);
            mailButton1.Visible = PermissionService.Instance().IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm);
            mailButton1.SubscribeToMessageEvent(_controller);

            _controller.MessageAdded += (sender, e) => ShowBalloon(false, e.Message, e.Title);
        }

        public void ShowBalloon(bool showOnlyMinimized, string message, string title)
        {
            //Anders wants the balloon everywhere
            //if (showOnlyMinimized && WindowState != FormWindowState.Minimized) return;
            if (showOnlyMinimized) return;
            notifyIconScheduleMessenger.BalloonTipIcon = ToolTipIcon.Info;
            notifyIconScheduleMessenger.BalloonTipText = message;
            notifyIconScheduleMessenger.BalloonTipTitle = title;
            notifyIconScheduleMessenger.ShowBalloonTip(500);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitializeMessenger();
            SetPermissions();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _caller.Close();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (WindowState == FormWindowState.Minimized)
                Hide();

            if (WindowState == FormWindowState.Maximized)
            {
                if (_caller != null)
                {
                    mailButton1.CloseMessageForm();
                    if (Visible) Hide();

                    _caller.Show();
                    if (_updated)
                        _caller.RefreshTab();
                    _caller.WindowState = FormWindowState.Maximized;
                   
                }
            }
        }

        /// <summary>
        /// Handles the DoubleClick event of the notifyIconScheduleMessenger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/3/2008
        /// </remarks>
        private void notifyIconScheduleMessenger_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/3/2008
        /// </remarks>
        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemRestoreASM control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/3/2008
        /// </remarks>
        private void toolStripMenuItemRestoreASM_Click(object sender, EventArgs e)
        {
            if (_caller != null)
            {
                if (_caller.Visible) _caller.Hide();
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemRestoreAgentPortal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-11
        /// </remarks>
        private void toolStripMenuItemRestoreAgentPortal_Click(object sender, EventArgs e)
        {
            if (_caller != null)
            {
                if (Visible) Hide();
                _caller.Show();
                _caller.WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>
        /// Handles the BallonTipClick event of the notifyIconScheduleMessenger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 9/3/2008
        /// </remarks>
        private void notifyIconScheduleMessenger_BallonTipClick(object sender, EventArgs e)
        {
            if (_caller != null)
            {
                if (_caller.Visible) _caller.Hide();
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Initilaizes the messenger.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/3/2008
        /// </remarks>
        private void InitializeMessenger()
        {
            //Sartup location of control should be at top middle of the screen
            int screenMidPoint = Screen.PrimaryScreen.WorkingArea.Width / 2;
            int controlMidPoint = Width / 2;
            IsMdiContainer = true;
            Location = new Point((screenMidPoint - controlMidPoint), 0);
			loadScheduleData();
			redrawSchedule();
            AgentScheduleStateHolder.Instance().CheckNextActivity();
            _messageBrokerHandler.ScheduleChanged += messageBrokerHandler_ScheduleChanged;
            _messageBrokerHandler.StartMessageBrokerListener();
            timerCurrentTime.Start();
        }

        private void messageBrokerHandler_ScheduleChanged(object sender, EventArgs e)
        {
        	loadScheduleData();
			redrawSchedule();
            AgentScheduleStateHolder.Instance().CheckNextActivity();

            //Show balloon tip
            string message = UserTexts.Resources.BallonTipScheduleChange;
            string title = UserTexts.Resources.AgentScheduleMessenger;
            ShowBalloon(false, message, title);
            _updated = true;
        }

        private void loadScheduleData()
        {
            try
            {
                _scheduleView.LoadScheduleMessengerSchedule(DateTime.UtcNow);
            	_scheduleLoadRetryCount = 0;
            }
            catch (TimeoutException timeoutException)
            {
                MessageDialogs.ShowError(this,timeoutException.Message,Text);
            }
			catch(FaultException)
			{
				_scheduleLoadRetryCount++;

				if (_scheduleLoadRetryCount>3)
				{
					throw;
				}

				Thread.Sleep(TimeSpan.FromSeconds(7*_scheduleLoadRetryCount));
				loadScheduleData();
			}
        }

    	private void redrawSchedule()
    	{
    		IList<ICustomScheduleAppointment> filteredScheduleAppointments = GetFilteredScheduleAppointments();
    		DateTime startDateTime = DateTime.Today.Add(TimeSpan.FromHours(7));
    		DateTime endDateTime = startDateTime.AddHours(11);
    		if (filteredScheduleAppointments.Count > 0)
    		{
    			HandleDayOff(filteredScheduleAppointments);
    			startDateTime = filteredScheduleAppointments[0].StartTime;
    			if (startDateTime.Minute == 0)
    				startDateTime = startDateTime.AddHours(-1);
    			startDateTime = startDateTime.AddMinutes(-startDateTime.Minute).AddSeconds(-startDateTime.Second);
    			endDateTime = filteredScheduleAppointments[filteredScheduleAppointments.Count - 1].EndTime.AddHours(2);
    			endDateTime = endDateTime.AddMinutes(-endDateTime.Minute).AddSeconds(-endDateTime.Second);
    		}
    		layerVisualizer1.SetControlDateTimePeriod(startDateTime, endDateTime);
    		layerVisualizer1.SetTimeBarDateTime(DateTime.Now);
    		layerVisualizer1.SetDelayedLayerCollection(filteredScheduleAppointments);
    	}

    	private static IList<ICustomScheduleAppointment> GetFilteredScheduleAppointments()
        {
            var scheduleAppointments =
                AgentScheduleStateHolder.Instance().ScheduleMessengerScheduleDictionary.AllScheduleAppointments();
            scheduleAppointments.SortStartTime();
            IList<ICustomScheduleAppointment> filteredScheduleAppointments = new List<ICustomScheduleAppointment>();
            foreach (ICustomScheduleAppointment scheduleAppointment in scheduleAppointments)
            {
                if (scheduleAppointment.AppointmentType == ScheduleAppointmentTypes.Request ||
                   scheduleAppointment.AppointmentType == ScheduleAppointmentTypes.PublicNote) continue;

                filteredScheduleAppointments.Add(scheduleAppointment);
            }
            return filteredScheduleAppointments;
        }

        private static void HandleDayOff(IList<ICustomScheduleAppointment> appointments)
        {
            if (appointments.Count != 1) return;

            ICustomScheduleAppointment appointment = appointments[0];
            if (appointment == null || appointment.StartTime == appointment.EndTime || appointment.AppointmentType != ScheduleAppointmentTypes.DayOff) return;

            appointment.StartTime = appointment.StartTime.AddHours(-6);
            appointment.EndTime = appointment.EndTime.AddHours(6);
        }

        /// <summary>
        /// Sets the permissions.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-05
        /// </remarks>  
        private void SetPermissions()
        {
            toolStripMenuItemRestoreASM.Visible = PermissionService.Instance().IsPermitted(
                ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.OpenAsm);
        }

        private void UnhookEvents()
        {
            _messageBrokerHandler.UnregisterMessageBrokerSubscriptions();
            timerCurrentTime.Stop();
            timerCurrentTime.Tick -= timerCurrentTime_Tick;
        }

        private void timerCurrentTime_Tick(object sender, EventArgs e)
        {
            layerVisualizer1.SetTimeBarDateTime(DateTime.Now);
            AgentScheduleStateHolder.Instance().CheckNextActivity();
        }
    }
}