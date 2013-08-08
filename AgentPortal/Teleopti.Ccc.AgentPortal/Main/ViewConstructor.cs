using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView;
using Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Reports;
using Teleopti.Ccc.AgentPortal.AgentSchedule;
using Teleopti.Ccc.AgentPortal.Requests;
using Teleopti.Ccc.AgentPortal.Requests.RequestMaster;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster;

namespace Teleopti.Ccc.AgentPortal.Main
{
    /// <summary>
    /// View controller for the AgenPortal
    /// </summary>
    /// <remarks>
    /// Created by: kosalanp
    /// Created date: 5/16/2008
    /// </remarks>
    public class ViewConstructor : IDisposable
    {
        private static ViewConstructor _instance = new ViewConstructor();
        private Control _hostedView;
        private ScheduleControl _scheduleControl;
        private PreferenceView _preferenceView;
        private StudentAvailabilityView _studentAvailabilityView;
	    private readonly LegendLoader _legendLoader = new LegendLoader(()=>SdkServiceHelper.SchedulingService);

	    private ViewConstructor()
        {
        }

        public event EventHandler<EventArgs> ScheduleViewChanged;

        public static ViewConstructor Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ViewConstructor();

                return _instance;
            }
        }

        public Control HostedView
        {
            get { return _hostedView; }
        }

       PreferenceView GetPreferenceView(IToggleButtonState parent)
       {
           if (_preferenceView == null)
               _preferenceView = new PreferenceView(parent);

           return _preferenceView;
       }
        public Control BuildPortalView(ViewType type, Control container, IToggleButtonState parent)
        {
            if (_scheduleControl == null)
            {
                _scheduleControl = new ScheduleControl(parent, _legendLoader);
                _scheduleControl.ViewChanged += scheduleControl_ViewChanged;
            }
            if (container != null)
            {
                switch (type)
                {
                    case ViewType.Schedule:
                        _hostedView = _scheduleControl;
                        _scheduleControl.ScheduleView.Refresh();
                        break;

                    case ViewType.Legend:
                        _hostedView = new LegendsView(_legendLoader);
                        ((XPTaskBarBox)container.Parent).PreferredChildPanelHeight = ((LegendsView)_hostedView).DefaultHeight;
                        //height vary with no. of different activities
                        break;

                    case ViewType.Scorecard:

                        _hostedView = MatrixAgent.Instance.ScoreCard;
                        break;

                    case ViewType.Report:

                        _hostedView = MatrixAgent.Instance.GetMatrixReport();
                        break;

                    case ViewType.Requests:
                        RequestMasterModel model = new RequestMasterModel(SdkServiceHelper.SchedulingService, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson);
                        _hostedView = new RequestMasterView(model);
                        break;

                    case ViewType.MyReport:

                        _hostedView = new MyReportControl();
                        break;
                    case ViewType.Preference:
                        _hostedView = GetPreferenceView(parent);
                        break;
                    case ViewType.StudentAvailable:
                        _hostedView = GetStudentAvailabilityView(parent);
                        break;
                    case ViewType.Accounts:
                        PersonAccountView personAccountView = new PersonAccountView(SdkServiceHelper.OrganizationService, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson);
                        _hostedView = personAccountView;
                        break;
                }

                if (_hostedView != null)
                {
                    _hostedView.Dock = DockStyle.Fill;
                    _hostedView.Top = 0;
                    _hostedView.Left = 0;
                    _hostedView.Visible = true;
                    _hostedView.Width = container.Width;
                    _hostedView.Height = container.Height;

                    if (_hostedView is StudentAvailabilityView || _hostedView is PreferenceView)
                        parent.ShowRightPanel(false);
                    else parent.ShowRightPanel(true);

                    //Clear all controls and host current view.
                    if (container.Controls.Count > 0) UnregisterScheduleControl(container.Controls[0]);
                    container.Controls.Clear();
                    container.Controls.Add(_hostedView);
                }
                return _hostedView;
            }

            return null;
        }

        private StudentAvailabilityView GetStudentAvailabilityView(IToggleButtonState parent)
        {
            if (_studentAvailabilityView == null)
                _studentAvailabilityView = new StudentAvailabilityView(parent);

            return _studentAvailabilityView;
        }

        void scheduleControl_ViewChanged(object sender, EventArgs e)
        {
            if (ScheduleViewChanged != null)
                ScheduleViewChanged(sender, e);
        }

        #region IDisposable Members

        public void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);
            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                UnregisterScheduleControl(_hostedView);
                UnregisterScheduleControl(_scheduleControl);
                _scheduleControl.Dispose();
                _hostedView.Dispose();
                _instance = null;
                _preferenceView.Dispose();
                _preferenceView = null;
                _studentAvailabilityView.Dispose();
                _studentAvailabilityView = null;
            }
        }

        private static void UnregisterScheduleControl(Control control)
        {
            ScheduleControl scheduleControl = control as ScheduleControl;
            if (scheduleControl != null) scheduleControl.UnregisterForMessageBrokerEvents();
        }

        #endregion
    }
}