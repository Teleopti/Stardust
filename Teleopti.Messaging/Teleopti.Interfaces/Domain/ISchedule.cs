using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A person's schedule a certain period
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-25
    /// </remarks>
    public interface ISchedule : IScheduleParameters, ICloneable
    {
        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-10
        /// </remarks>
        IScheduleDictionary Owner { get; }

        /// <summary>
        /// Determines whether [contains] [the specified schedule data].
        /// </summary>
        /// <param name="scheduleData">The schedule data.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified schedule data]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-26
        /// </remarks>
        bool Contains(IScheduleData scheduleData);


        /// <summary>
        /// Gets the business rule response internal collection.
        /// </summary>
        /// <value>The business rule response internal collection.</value>
        IList<IBusinessRuleResponse> BusinessRuleResponseInternalCollection { get; }
    }
}