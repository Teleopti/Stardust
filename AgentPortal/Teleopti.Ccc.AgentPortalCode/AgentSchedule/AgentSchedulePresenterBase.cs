using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class AgentSchedulePresenterBase
    {
        private IAgentScheduleViewBase          _view;
        private readonly AgentScheduleStateHolder        _scheduleStateHolder;

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        public IAgentScheduleViewBase View
        {
            get { return _view; }
            set { _view = value; }
        }

        /// <summary>
        /// Gets the schedule state holder.
        /// </summary>
        /// <value>The schedule state holder.</value>
        public AgentScheduleStateHolder ScheduleStateHolder
        {
            get { return _scheduleStateHolder; }
        }

        /// <summary>
        /// Sets the color theme.
        /// </summary>
        /// <param name="colorTheme">The color theme.</param>
        public void SetColorTheme(ScheduleAppointmentColorTheme colorTheme)
        {
            _scheduleStateHolder.ScheduleAppointmentColorTheme = colorTheme;
        }

        /// <summary>
        /// Adds the visualization filter.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public void AddVisualizationFilter(ScheduleAppointmentTypes filterType,bool enable)
        {
            if (enable)
            {
                _scheduleStateHolder.VisualizingScheduleAppointmentTypes |= filterType;
            }
            else
            {
                _scheduleStateHolder.VisualizingScheduleAppointmentTypes &= ~filterType;
            }
            _view.Refresh();
        }

        /// <summary>
        /// Sets the resolution.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        public void SetResolution(int resolution)
        {
            if (resolution > 0)
            {
                _scheduleStateHolder.CurrentResolution = resolution;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentSchedulePresenterBase"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="stateHolder">The state holder.</param>
        public AgentSchedulePresenterBase(IAgentScheduleViewBase view, AgentScheduleStateHolder stateHolder)
        {
            View = view;
            _scheduleStateHolder = stateHolder;
        }
    }
}
