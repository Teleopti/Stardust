using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-11-17    
    /// /// </remarks>
    public interface ISkillStaffPeriodDataHolder : IWorkShiftCalculatableSkillStaffPeriod
    {
        /// <summary>
        /// Gets or sets the original demand in minutes.
        /// </summary>
        /// <value>The original demand in minutes.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        double OriginalDemandInMinutes { get; set; }
        /// <summary>
        /// Gets or sets the assigned resource in minutes.
        /// </summary>
        /// <value>The assigned resource in minutes.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        double AssignedResourceInMinutes { get; set; }
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        DateTimePeriod Period { get; }
        /// <summary>
        /// Gets or sets the minimum persons.
        /// </summary>
        /// <value>The minimum persons.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        int MinimumPersons { get; set; }
        /// <summary>
        /// Gets or sets the maximum persons.
        /// </summary>
        /// <value>The maximum persons.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        int MaximumPersons { get; set; }

        /// <summary>
        /// Gets or sets the absolute difference scheduled heads and min max heads.
        /// </summary>
        /// <value>The absolute difference scheduled heads and min max heads.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-02-25    
        /// /// </remarks>
        double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads { get; set; }

        /// <summary>
        /// Gets the period distribution.
        /// </summary>
        /// <value>The period distribution.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-05-05    
        /// /// </remarks>
        IPeriodDistribution PeriodDistribution { get; }

        /// <summary>
        /// Gets or sets the tweaked current demand.
        /// </summary>
        /// <value>The tweaked current demand.</value>
        double TweakedCurrentDemand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ISkillStaffPeriodDataHolder"/> is boosted.
        /// First and last interval will get a boost to assure they are scheduled first
        /// </summary>
        /// <value><c>true</c> if boost; otherwise, <c>false</c>.</value>
        bool Boost { get; set; }
    }
}