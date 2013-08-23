using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{

    /// <summary>
    /// Represnt the base class for all schedule views
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2009-01-19
    /// </remarks>
    public class AgentScheduleViewBase<TPresenter> : IAgentScheduleViewBase where TPresenter : AgentSchedulePresenterBase
    {
        private TPresenter _presenter;

        /// <summary>
        /// Gets or sets the presenter.
        /// </summary>
        /// <value>The presenter.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        public TPresenter Presenter
        {
            get { return _presenter; }
            set { _presenter = value; }
        }

        /// <summary>
        /// Refreshes the specified reload data.
        /// </summary>
        /// <param name="reloadData">if set to <c>true</c> [reload data].</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        public virtual void Refresh()
        {
        }

        /// <summary>
        /// Sets the resolution.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        public virtual void SetResolution(int resolution)
        {
            _presenter.SetResolution(resolution);
        }

        /// <summary>
        /// Sets the color theme.
        /// </summary>
        /// <param name="colorTheme">The color theme.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        public virtual void SetColorTheme(ScheduleAppointmentColorTheme colorTheme)
        {
            _presenter.SetColorTheme(colorTheme);
        }

        /// <summary>
        /// Adds the visualization filter.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-19
        /// </remarks>
        public  void AddVisualizationFilter(ScheduleAppointmentTypes filterType,bool enable)
        {
            _presenter.AddVisualizationFilter(filterType, enable);
        }
    }
}
