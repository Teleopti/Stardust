using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// The class represents how one cell in Scheduler period view could be displayed.
    /// Zoë
    /// </summary>
    public class XCell
    {
        private readonly List<AgentAssignmentDisplay> _agentAssignmentDispList = new List<AgentAssignmentDisplay>();
        private readonly List<AgentAbsenceDisplay> _agentAbsenceDispList = new List<AgentAbsenceDisplay>();
        private DayOffDisplay _dayOff;
        private bool _hasPersonalShift;
        private AgentDate _key;
        private IList<XCell> referencedXCells = new List<XCell>();



        /// <summary>
        /// Initializes a new instance of the <see cref="XCell"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="agentAbsenceDictionary">The agent absence dictionary.</param>
        /// <param name="agentAssignmentDictionary">The agent assignment dictionary.</param>
        /// <param name="agentDayOffDictionary">The agent day off dictionay.</param>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-09
        /// </remarks>
        public XCell(AgentDate key, IDictionary<AgentDate, DailyAgentAbsenceCollection> agentAbsenceDictionary,
                                    IDictionary<AgentDate, DailyAgentAssignmentCollection> agentAssignmentDictionary, 
                                    IDictionary<AgentDate, PersonDayOff> agentDayOffDictionary, XCellCache parent)
        {
            _key = key;
            AddAbsences(agentAbsenceDictionary, parent);
            AddAssignment(agentAssignmentDictionary, parent);
            AddAgentDayOffs(agentDayOffDictionary);
        }

        /// <summary>
        /// Adds the agent day offs.
        /// </summary>
        /// <param name="agentDayOffDictionay">The agent day off dictionay.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-09
        /// </remarks>
        private void AddAgentDayOffs(IDictionary<AgentDate, PersonDayOff> agentDayOffDictionay)
        {
            PersonDayOff personDayOff = null;
            bool found = agentDayOffDictionay.TryGetValue(Key, out personDayOff);
            if (!found)
            {
                return;
            }

            XCell xCell = this;

            DayOffDisplay dayOffDisplay = new DayOffDisplay(personDayOff);//, personDayOff.DayOff.Anchor);
            xCell._dayOff = dayOffDisplay;
        }

        /// <summary>
        /// Adds the absences to the XCell.
        /// </summary>
        /// <param name="agentAbsDic">The agent abs dic.</param>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-07
        /// </remarks>
        private void AddAbsences(IDictionary<AgentDate, DailyAgentAbsenceCollection> agentAbsDic, XCellCache parent)
        {
            DailyAgentAbsenceCollection collection = null;
            bool found = agentAbsDic.TryGetValue(Key, out collection);
            if (!found)
            {
                return;
            }

            foreach (PersonAbsence agentAbsence in collection)
            {
                IList<DateTime> dateList = agentAbsence.AffectedLocalDates();

                foreach (DateTime date in dateList)
                {
                    XCell xCell;
                    if (date.Date != Key.Date.Date)
                    {
                        xCell = parent.GetXCell(new AgentDate(Key.Agent, date.Date));
                        referencedXCells.Add(xCell);
                    }
                    else
                    {
                        xCell = this;
                    }
                    xCell.AgentAbsenceDisplayList.Add(new AgentAbsenceDisplay(agentAbsence,
                                                                              TimeZoneHelper.
                                                                                  NewDateTimePeriodFromLocalDateTime
                                                                                  (
                                                                                  date.Date,
                                                                                  date.Date.AddDays(1).
                                                                                      AddMilliseconds(
                                                                                      -1))));
                }
            }
        }

        /// <summary>
        /// Adds the assignment to the XCell.
        /// </summary>
        /// <param name="agentAssDic">The agent ass dic.</param>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-07
        /// </remarks>
        private void AddAssignment(IDictionary<AgentDate, DailyAgentAssignmentCollection> agentAssDic, XCellCache parent)
        {

            DailyAgentAssignmentCollection collection = null;
            bool found = agentAssDic.TryGetValue(Key, out collection);
            if (!found)
            {
                return;
            }
            foreach (PersonAssignment agentAssignment in collection)
            {
                //IList<DateTime> dateList = agentAssignment.Period().LocalDaysAffected();
                DateTime date = agentAssignment.Period().LocalStartDateTime.Date;
                //foreach (DateTime date in dateList)
                //{
                    XCell xCell;
                    if (date.Date != Key.Date.Date)
                    {
                        xCell = parent.GetXCell(new AgentDate(Key.Agent, date.Date));
                        referencedXCells.Add(xCell);
                    }
                    else
                    {
                        xCell = this;
                    }
                    xCell.AgentAssignmentDisplayList.Add(
                        new AgentAssignmentDisplay(agentAssignment,
                                                   TimeZoneHelper.NewDateTimePeriodFromLocalDateTime(date.Date,
                                                                                                     date.Date.AddDays(1)
                                                                                                         .
                                                                                                         AddMilliseconds
                                                                                                         (-1))));

                    if (agentAssignment.PersonalShiftCollection.Count > 0)
                    {
                        xCell._hasPersonalShift = true;
                    }
                //}
            }
        }

        /// <summary>
        /// Gets the agent assignment display list.
        /// </summary>
        /// <value>The agent assignment display list.</value>
        /// Zoë
        public IList<AgentAssignmentDisplay> AgentAssignmentDisplayList
        {
            get { return _agentAssignmentDispList; }
        }

        /// <summary>
        /// Gets the agent absence display list.
        /// </summary>
        /// <value>The agent absence display list.</value>
        /// Zoë
        public IList<AgentAbsenceDisplay> AgentAbsenceDisplayList
        {
            get { return _agentAbsenceDispList; }
        }

        /// <summary>
        /// Gets the day off.
        /// </summary>
        /// <value>The day off.</value>
        /// Zoë
        public DayOffDisplay DayOff
        {
            get { return _dayOff; }
        }

        /// <summary>
        /// Gets the scheduled date.
        /// </summary>
        /// <value>The scheduled date.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-06
        /// </remarks>
        public DateTime ScheduledDate
        {
            get { return Key.Date.Date; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has personal shift.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has personal shift; otherwise, <c>false</c>.
        /// </value>
        /// Zoë
        public bool HasPersonalShift
        {
            get { return _hasPersonalShift; }
        }

        /// <summary>
        /// Gets the agent.
        /// </summary>
        /// <value>The agent.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-06
        /// </remarks>
        public Person Agent
        {
            get { return Key.Agent; }
        }

        /// <summary>
        /// Gets a value indicating whether [affects other cell].
        /// </summary>
        /// <value><c>true</c> if [affects other cell]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-06
        /// </remarks>
        public bool AffectsOtherCell
        {
            get { return referencedXCells.Count > 0; }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        public AgentDate Key
        {
            get { return _key; }
        }
    }
}