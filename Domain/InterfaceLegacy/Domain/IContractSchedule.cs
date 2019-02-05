using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{

    /// <summary>
    /// ContractSchedule 
    /// </summary>
	public interface IContractSchedule : IAggregateRoot, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Description of ContractSchedule
        /// </summary>
        Description Description { get; set; }

        /// <summary>
        /// Gets the contained weeks in contract schedule
        /// </summary>
        IEnumerable<IContractScheduleWeek> ContractScheduleWeeks { get; }

        /// <summary>
        /// Adds the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void AddContractScheduleWeek(IContractScheduleWeek contractScheduleWeek);


        /// <summary>
        /// Removes the contract schedule week.
        /// </summary>
        /// <param name="contractScheduleWeek">The contract schedule week.</param>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void RemoveContractScheduleWeek(IContractScheduleWeek contractScheduleWeek);

        /// <summary>
        /// Clears the contract schedule week collection
        /// </summary>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        void ClearContractScheduleWeeks();

		bool IsWorkday(DateOnly personPeriodStartDate, DateOnly requestedDate, DayOfWeek dayOfWeek);
	}
}
