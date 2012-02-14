namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// This object supports moving to another scenario
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-11-23
    /// </remarks>
    public interface IExportToAnotherScenario : IPersistableScheduleData
    {
        /// <summary>
        /// Creates a clone and replacing IScheduleParameters.
        /// If cloning and changing parameters (for eg scenario export) shouldn't be possible, return null
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-10-12
        /// </remarks>
        IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters);
    }
}
