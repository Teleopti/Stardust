using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Class that holds XCells for each day in loaded area in scheduler
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2007-11-06
    /// </remarks>
    public class XCellCache
    {
        private Dictionary<AgentDate, XCell> xCellDictionary = new Dictionary<AgentDate, XCell>();
        private IDictionary<AgentDate, DailyAgentAbsenceCollection> _agentAbsenceDictionary = new Dictionary<AgentDate, DailyAgentAbsenceCollection>();
        private IDictionary<AgentDate, DailyAgentAssignmentCollection> _agentAssignmnetDictionary = new Dictionary<AgentDate, DailyAgentAssignmentCollection>();
        private IDictionary<AgentDate, PersonDayOff> _agentDayOffDictionary = new Dictionary<AgentDate, PersonDayOff>();


        /// <summary>
        /// Initializes a new instance of the <see cref="XCellCache"/> class.
        /// </summary>
        /// <param name="agentAbsenceDictionary">The agent absence dictionary.</param>
        /// <param name="agentAssignmentDictionary">The agent assignment dictionary.</param>
        /// <param name="agentDayOffDictionary">The agent day off dictionary.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-09
        /// </remarks>
        public XCellCache(IDictionary<AgentDate, DailyAgentAbsenceCollection> agentAbsenceDictionary, IDictionary<AgentDate, DailyAgentAssignmentCollection> agentAssignmentDictionary, IDictionary<AgentDate, PersonDayOff> agentDayOffDictionary)
        {
            _agentAbsenceDictionary = agentAbsenceDictionary;
            _agentAssignmnetDictionary = agentAssignmentDictionary;
            _agentDayOffDictionary = agentDayOffDictionary;
        }

        /// <summary>
        /// Gets the X cell.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-06
        /// </remarks>
        public XCell GetXCell(AgentDate key)
        {
            XCell xCell;

            xCellDictionary.TryGetValue(key, out xCell);

            if (xCell==null)
            {
                xCell = CreateXCell(key);
                xCellDictionary.Add(key, xCell);
            }
            return xCell;
        }

        private XCell CreateXCell(AgentDate key)
        {
            XCell xCell = new XCell(key, _agentAbsenceDictionary, _agentAssignmnetDictionary, _agentDayOffDictionary, this);
            return xCell;
        }

        /// <summary>
        /// Remove an XCell from the dictionary
        /// </summary>
        /// <param name="key"></param>
        public void RemoveXCell(AgentDate key)
        {
            if(xCellDictionary.ContainsKey(key))
            {
                xCellDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 13.12.2007
        /// </remarks>
        public void ClearCache()
        {
            xCellDictionary.Clear();
        }
    }
}
