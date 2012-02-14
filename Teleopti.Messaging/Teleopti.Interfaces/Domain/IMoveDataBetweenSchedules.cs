﻿using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// A service that moves data between two IScheduleDictionaries
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-10-12
    /// </remarks>
    public interface IMoveDataBetweenSchedules
    {
        /// <summary>
        /// Exports the specified persons and period.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="sourceParts">The source parts.</param>
        /// <returns>
        /// Returns the business rules that this method has overriden.
        /// Use it to show a warning of what has happened.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-10-12
        /// </remarks>
        IEnumerable<IBusinessRuleResponse> CopySchedulePartsToAnotherDictionary(IScheduleDictionary destination, IEnumerable<IScheduleDay> sourceParts);
    }

}
