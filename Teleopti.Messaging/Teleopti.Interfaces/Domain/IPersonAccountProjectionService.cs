using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Helper for deciding what to read from repository and what to read from loaded data
    /// </summary>
    public interface IPersonAccountProjectionService
    {
        /// <summary>
        /// Alls the visual layers from loaded data and repository.
        /// </summary>
        /// <param name="storage">The repository.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IList<IScheduleDay> CreateProjection(IScheduleStorage storage, IScenario scenario);
    }
}
