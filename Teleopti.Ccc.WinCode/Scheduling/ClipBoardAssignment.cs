using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Class that holds AgentAssignments, AgentAbsences and AgentDayOffs
    /// to simplify copy/paste and inserts 
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2007-11-16
    /// </remarks>
    public class ClipboardAssignment
    {
        private IList<PersonAssignment> agentAssignmentList = new List<PersonAssignment>();
        private IList<PersonAbsence> agentAbsenceList = new List<PersonAbsence>();
        private PersonDayOff personDayOff;
        private Person _person;
        private DateTime _date;

        /// <summary>
        /// Gets the agent assignment list.
        /// </summary>
        /// <value>The agent assignment list.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-16
        /// </remarks>
        public IList<PersonAssignment> AgentAssignmentList
        {
            get { return agentAssignmentList; }
        }

        /// <summary>
        /// Gets the agent absence list.
        /// </summary>
        /// <value>The agent absence list.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-16
        /// </remarks>
        public IList<PersonAbsence> AgentAbsenceList
        {
            get { return agentAbsenceList; }
        }

        /// <summary>
        /// Gets or sets the agent day off.
        /// </summary>
        /// <value>The agent day off.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-16
        /// </remarks>
        public PersonDayOff PersonDayOff
        {
            get { return personDayOff; }
            set { personDayOff = value; }
        }

        /// <summary>
        /// Gets or sets the Agent
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-02-20
        /// </remarks>
        public Person Agent
        {
            get { return _person; }
            set { _person = value; }
        }

        /// <summary>
        /// Get or sets the scheduled date
        /// </summary>
        /// /// <remarks>
        /// Created by: cs
        /// Created date: 2008-02-20
        /// </remarks>
        public DateTime ScheduledDate
        {
            get { return _date; }
            set { _date = value; }
        }
    }
}
