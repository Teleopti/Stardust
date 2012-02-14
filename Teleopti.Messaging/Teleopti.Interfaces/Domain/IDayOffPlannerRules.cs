using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for DayOffOptimizerRules
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-02    
    /// /// </remarks>
    public interface IDayOffPlannerRules : ICloneable
    {
        /// <summary>
        /// Gets or sets the number of day offs in the period.
        /// </summary>
        /// <value>The number of day offs in the period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        int NumberOfDayOffsInPeriod { get; set; }

        /// <summary>
        /// Gets or sets the number of days in the period.
        /// </summary>
        /// <value>The number of days in the period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        int NumberOfDaysInPeriod { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Indicates whether the planner should take WorkDaysBetween into consideration
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        bool UseConsecutiveWorkdays { get; set; }

        /// <summary>
        /// Returns the limits for the number of working days between two day off's
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        MinMax<int> ConsecutiveWorkdays { get; set; }

  
        /// <summary>
        /// Indicates whether the planner should take DaysOffPerWeek into consideration
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        bool UseDaysOffPerWeek { get; set; }

        /// <summary>
        /// Returns the number of day off's per week
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        MinMax<int> DaysOffPerWeek { get; set; }

        /// <summary>
        /// Indicates whether the planner should take ConsecutiveDaysOff into consideration
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        bool UseConsecutiveDaysOff { get; set; }

        /// <summary>
        /// Gets or sets the consecutive days off, a MinMax structure telling the rules of how many days off in a row are allowed
        /// </summary>
        /// <value>The consecutive days off.</value>
        MinMax<int> ConsecutiveDaysOff { get; set; }
        
        /// <summary>
        /// Gets or sets an indicator if the planner should consider days off planned before the start date of this period
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        bool UsePreWeek { get; set; }

        /// <summary>
        /// Gets or sets an indicator if the planner should consider days off planned after the period
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        bool UsePostWeek { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use free weekends].
        /// </summary>
        /// <value><c>true</c> if [use free weekends]; otherwise, <c>false</c>.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        bool UseFreeWeekends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use free weekend days].
        /// </summary>
        /// <value><c>true</c> if [use free weekend days]; otherwise, <c>false</c>.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        bool UseFreeWeekendDays { get; set; }

        /// <summary>
        /// Gets or sets the free weekends.
        /// </summary>
        /// <value>The free weekends.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        MinMax<int> FreeWeekends { get; set; }

        /// <summary>
        /// Gets or sets the free weekend days.
        /// </summary>
        /// <value>The free weekend days.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-01    
        /// /// </remarks>
        MinMax<int> FreeWeekendDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum numbers of moved days.
        /// </summary>
        /// <value>The move max days.</value>
        int MaximumMovableDayOffsPerPerson { get; set; }

        /// <summary>
        /// Gets a value indicating whether to use move max days.
        /// </summary>
        /// <value><c>true</c> if use move max days; otherwise, <c>false</c>.</value>
        bool UseMoveMaxDays { get; }

        /// <summary>
        /// Gets or sets a value indicating whether both days in the weekend should be either free or workdays.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if weekends are kept together; otherwise, <c>false</c>.
        /// </value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-05    
        /// /// </remarks>
        bool KeepWeekendsTogether { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we want to keep free weekends during optimization.
        /// </summary>
        /// <value><c>true</c> if keep free weekends; otherwise, <c>false</c>.</value>
        bool KeepFreeWeekends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we want to keep free weekend days during optimization.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if keep free weekend days; otherwise, <c>false</c>.
        /// </value>
        bool KeepFreeWeekendDays { get; set; }
    }
}