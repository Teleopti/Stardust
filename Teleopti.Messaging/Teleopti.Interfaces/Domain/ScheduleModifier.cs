namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// What modifies the scheduler data?
    /// </summary>
    public enum ScheduleModifier
    {
        /// <summary>
        /// Scheduler has modified/is modifying the data
        /// </summary>
        Scheduler,
        /// <summary>
        /// Request form has modified/is modifying the data
        /// </summary>
        Request,
        /// <summary>
        /// Message broker has modified/is modifying the data
        /// </summary>
        MessageBroker,
        /// <summary>
        /// An undo or redo event has modified/is modifying the data
        /// </summary>
        UndoRedo,
        /// <summary>
        /// An automatic scheduling event has modifyed the data
        /// </summary>
        AutomaticScheduling,

        /// <summary>
        /// Export to another scenario is modifying
        /// </summary>
        ScenarioExport
    }
}
