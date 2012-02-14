using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Information about the agent's current state
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-06
    /// </remarks>
    public interface IAgentState
    {
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        IPerson Person { get; }

        /// <summary>
        /// Gets the rta visual layer collection.
        /// </summary>
        /// <value>The rta visual layer collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        ReadOnlyCollection<IRtaVisualLayer> RtaVisualLayerCollection { get; }

        /// <summary>
        /// Gets the alarm situation collection.
        /// </summary>
        /// <value>The alarm situation collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        ReadOnlyCollection<IVisualLayer> AlarmSituationCollection { get; }

        /// <summary>
        /// Adds the rta visual layer.
        /// </summary>
        /// <param name="rtaVisualLayer">The rta visual layer.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        void AddRtaVisualLayer(IRtaVisualLayer rtaVisualLayer);

        /// <summary>
        /// Gets the layer start date time.
        /// </summary>
        /// <param name="externalAgentState">State of the external agent.</param>
        /// <param name="rtaState">State of the rta.</param>
        /// <param name="dummyActivityForState">State of the dummy activity for.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        void LengthenOrCreateLayer(IExternalAgentState externalAgentState, IRtaState rtaState, IActivity dummyActivityForState);

        /// <summary>
        /// Analyzes the alarm situations and adds them to the list
        /// </summary>
        /// <param name="stateGroupActivityAlarms">The state group activity alarms.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-17
        /// </remarks>
        void AnalyzeAlarmSituations(IEnumerable<IStateGroupActivityAlarm> stateGroupActivityAlarms, DateTime timestamp);

        /// <summary>
        /// Updates the current layer.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="refreshRate">The refresh rate.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-20
        /// </remarks>
        void UpdateCurrentLayer(DateTime timestamp, TimeSpan refreshRate);

        /// <summary>
        /// Finds the current alarm.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-24
        /// </remarks>
        IVisualLayer FindCurrentAlarm(DateTime timestamp);

        /// <summary>
        /// Finds the state of the current.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-24
        /// </remarks>
        IRtaVisualLayer FindCurrentState(DateTime timestamp);

        /// <summary>
        /// Logs the out.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-06-14
        /// </remarks>
        void LogOff(DateTime timestamp);

        /// <summary>
        /// Sets the schedule for comparison while creating alarm situations.
        /// </summary>
        /// <param name="scheduleDictionary">The schedule dictionary.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-06-27
        /// </remarks>
        void SetSchedule(IScheduleDictionary scheduleDictionary);

        /// <summary>
        /// Clears the alarm situations.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-08-05
        /// </remarks>
        void ClearAlarmSituations();

        /// <summary>
        /// Finds the current schedule.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-23
        /// </remarks>
        IVisualLayer FindCurrentSchedule(DateTime timestamp);

        /// <summary>
        /// Finds the next schedule.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-23
        /// </remarks>
        IVisualLayer FindNextSchedule(DateTime timestamp);
    }
}