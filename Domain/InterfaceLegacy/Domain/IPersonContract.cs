using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a concrete contract for a person (agent). Contains shedule and
    /// part time information.
    /// </summary>
    public interface IPersonContract : ICloneable
    {
        /// <summary>
        /// Contract for employment
        /// </summary>
        IContract Contract { get; set; }

        /// <summary>
        /// The percentage for part time calculations etc.
        /// </summary>
        IPartTimePercentage PartTimePercentage { get; set; }

        /// <summary>
        /// Basic schedule for absence calculation and basic information for scheduling period
        /// </summary>
        IContractSchedule ContractSchedule { get; set; }

        /// <summary>
        /// Gets the average work time per day with part time percentage applied.
        /// </summary>
        /// <value>The average work time per day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        TimeSpan AverageWorkTimePerDay { get; } 
    }

}
