using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    /// <summary>
    /// Reperent the View of Agent Team view
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2009-01-21
    /// </remarks>
    public class AgentScheduleTeamView : AgentScheduleViewBase<AgentScheduleTeamPresenter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentScheduleTeamView"/> class.
        /// </summary>
        /// <param name="stateHolder">The state holder.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-21
        /// </remarks>
        public AgentScheduleTeamView(AgentScheduleStateHolder stateHolder)
        {
            Presenter = new AgentScheduleTeamPresenter(this, stateHolder);
        }
    }
}
