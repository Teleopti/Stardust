using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    /// <summary>
    /// This is to hold data which is displayed in the team member grid
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-03-19
    /// </remarks>
    public class TeamMemberGridItem
    {
        private readonly List<ICustomScheduleAppointment> _scheduleItemCollection;

        public TeamMemberGridItem()
        {
            _scheduleItemCollection = new List<ICustomScheduleAppointment>();
        }

        public PersonDto PersonDto { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<ICustomScheduleAppointment> ScheduleItemCollection
        {
            get { return _scheduleItemCollection; }
        }
    }
}