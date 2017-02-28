namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Schedule parameters used by 
    /// PersonAssignment, PersonAbsence and Schedule
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-06
    /// </remarks>
    public interface IScheduleParameters : IPeriodized
    {
        /// <summary>
        /// Gets the scheduled person.
        /// </summary>
        /// <value>The scheduled person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        IPerson Person { get; }

        /// <summary>
        /// Gets the scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        IScenario Scenario { get;}
    }
}